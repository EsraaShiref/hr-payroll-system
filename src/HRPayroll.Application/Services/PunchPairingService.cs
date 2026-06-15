using HRPayroll.Domain.Entities;
using HRPayroll.Domain.Enums;

namespace HRPayroll.Application.Services;

/// <summary>
/// Option C: Sequential pairing with orphan handling.
/// Sorts all punches by timestamp, pairs alternating In→Out, recovers from
/// orphaned punches gracefully. Orphans are surfaced in the Exceptions Report.
/// </summary>
public class PunchPairingService
{
    public PunchPairingResult Pair(IReadOnlyList<AttendancePunch> punches)
    {
        if (punches.Count == 0)
            return new PunchPairingResult();

        var sorted = punches
            .OrderBy(p => p.TimestampUtc)
            .ToList();

        var sessions = new List<(AttendancePunch In, AttendancePunch Out)>();
        var orphans = new List<AttendancePunch>();
        AttendancePunch? openSession = null;

        foreach (var punch in sorted)
        {
            if (punch.Type == PunchType.In)
            {
                if (openSession != null)
                {
                    // Two In punches in a row — orphan the first (never clocked out)
                    orphans.Add(openSession);
                }
                openSession = punch;
            }
            else // Out
            {
                if (openSession != null)
                {
                    // Normal pair: close the session
                    sessions.Add((openSession, punch));
                    openSession = null;
                }
                else
                {
                    // Orphan Out — no matching In
                    orphans.Add(punch);
                }
            }
        }

        // If still inside a session at end of day, orphan the open In
        if (openSession != null)
            orphans.Add(openSession);

        var firstPunchIn = sessions.Count > 0
            ? TimeOnly.FromDateTime(sessions[0].In.TimestampUtc)
            : (TimeOnly?)null;

        var orphanIn = orphans.FirstOrDefault(o => o.Type == PunchType.In);
        if (orphanIn != null)
            firstPunchIn ??= TimeOnly.FromDateTime(orphanIn.TimestampUtc);

        var lastPunchOut = sessions.Count > 0
            ? TimeOnly.FromDateTime(sessions[^1].Out.TimestampUtc)
            : (TimeOnly?)null;

        var orphanOut = orphans.LastOrDefault(o => o.Type == PunchType.Out);
        if (orphanOut != null)
            lastPunchOut ??= TimeOnly.FromDateTime(orphanOut.TimestampUtc);

        var totalBreakMinutes = 0;
        for (int i = 1; i < sessions.Count; i++)
        {
            var gap = (int)(sessions[i].In.TimestampUtc - sessions[i - 1].Out.TimestampUtc).TotalMinutes;
            if (gap > 0)
                totalBreakMinutes += gap;
        }

        return new PunchPairingResult
        {
            FirstPunchIn = firstPunchIn,
            LastPunchOut = lastPunchOut,
            TotalBreakMinutes = totalBreakMinutes,
            OrphanPunches = orphans,
        };
    }
}
