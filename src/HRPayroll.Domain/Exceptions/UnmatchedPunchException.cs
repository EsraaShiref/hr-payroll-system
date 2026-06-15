namespace HRPayroll.Domain.Exceptions;

public class UnmatchedPunchException(string rowDetails, string reason)
    : DomainException($"Unmatched punch: {reason}. Row: {rowDetails}")
{
    public string RowDetails => rowDetails;
    public string Reason => reason;
}
