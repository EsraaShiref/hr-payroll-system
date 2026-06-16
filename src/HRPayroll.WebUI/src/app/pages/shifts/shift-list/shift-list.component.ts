import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { ShiftService, ShiftDto } from '../../../core/services/shift-holiday.service';

const DAY_LABELS = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
const DAY_VALUES = [1, 2, 4, 8, 16, 32, 64];

@Component({
  selector: 'app-shift-list',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatCardModule, MatTableModule, MatButtonModule, MatIconModule, MatChipsModule,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Shifts</h1>
        <button mat-raised-button color="primary" routerLink="/shifts/new">
          <mat-icon>add</mat-icon> New Shift
        </button>
      </div>

      <mat-card>
        <mat-card-content>
          <table mat-table [dataSource]="shifts()" class="full-width">
            <ng-container matColumnDef="name">
              <th mat-header-cell *matHeaderCellDef>Name</th>
              <td mat-cell *matCellDef="let s">
                <a [routerLink]="['/shifts', s.id]">{{ s.name }}</a>
              </td>
            </ng-container>

            <ng-container matColumnDef="time">
              <th mat-header-cell *matHeaderCellDef>Time</th>
              <td mat-cell *matCellDef="let s">{{ s.startTime }} — {{ s.endTime }}</td>
            </ng-container>

            <ng-container matColumnDef="workingDays">
              <th mat-header-cell *matHeaderCellDef>Working Days</th>
              <td mat-cell *matCellDef="let s">
                @for (day of getWorkingDays(s.workingDays); track day) {
                  <span class="day-chip">{{ day }}</span>
                }
              </td>
            </ng-container>

            <ng-container matColumnDef="thresholds">
              <th mat-header-cell *matHeaderCellDef>Thresholds</th>
              <td mat-cell *matCellDef="let s" class="thresholds-cell">
                Late: {{ s.lateThresholdMinutes }}m &nbsp; Early: {{ s.earlyDepartureThresholdMinutes }}m
                &nbsp; OT: {{ s.overtimeThresholdMinutes }}m
              </td>
            </ng-container>

            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef></th>
              <td mat-cell *matCellDef="let s">
                <button mat-icon-button [routerLink]="['/shifts', s.id]" matTooltip="Edit">
                  <mat-icon>edit</mat-icon>
                </button>
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="columns"></tr>
            <tr mat-row *matRowDef="let row; columns: columns;"></tr>
          </table>

          @if (shifts().length === 0) {
            <div class="empty-state">No shifts defined yet.</div>
          }
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .page-container { padding: 1.5rem; }
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; }
    .page-header h1 { margin: 0; font-size: 1.5rem; font-weight: 400; }
    .full-width { width: 100%; }
    .day-chip { display: inline-block; margin-right: 0.25rem; }
    .thresholds-cell { font-size: 0.85rem; white-space: nowrap; }
    .empty-state { text-align: center; padding: 2rem; color: var(--mat-sys-on-surface-variant, #666); }
  `],
})
export class ShiftListComponent implements OnInit {
  private shiftService = inject(ShiftService);

  shifts = signal<ShiftDto[]>([]);
  columns = ['name', 'time', 'workingDays', 'thresholds', 'actions'];

  ngOnInit(): void {
    this.shiftService.getList().subscribe(s => this.shifts.set(s));
  }

  getWorkingDays(value: number): string[] {
    return DAY_VALUES.filter(v => (value & v) !== 0).map(v => DAY_LABELS[DAY_VALUES.indexOf(v)]);
  }
}
