import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { AttendanceService } from '../../../core/services/attendance.service';
import { EmployeeService } from '../../../core/services/employee.service';
import { EmployeeDto } from '../../../models/hr';
import { AttendanceViewerResult } from '../../../models/attendance';

const STATUS_COLORS: Record<string, string> = {
  OnTime: 'primary',
  Late: 'warn',
  EarlyDeparture: 'warn',
  AbsentUnexcused: 'warn',
  OnLeave: 'accent',
  Holiday: 'accent',
  PendingReview: '',
};

@Component({
  selector: 'app-attendance-viewer',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterModule,
    MatCardModule, MatTableModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatButtonModule, MatIconModule, MatChipsModule,
    MatAutocompleteModule,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Attendance Viewer</h1>
      </div>

      <mat-card class="filters-card">
        <mat-card-content class="filters">
          <mat-form-field appearance="outline" subscriptSizing="dynamic" class="employee-field">
            <mat-label>Employee</mat-label>
            <input
              matInput
              [matAutocomplete]="auto"
              [(ngModel)]="searchTerm"
              (input)="filterEmployees()"
              placeholder="Search by name or code"
            />
            <mat-autocomplete #auto="matAutocomplete" [displayWith]="displayFn">
              @for (e of filteredEmployees(); track e.id) {
                <mat-option [value]="e" (onSelectionChange)="onEmployeeSelected(e)">
                  {{ e.fullName }} ({{ e.employeeCode }})
                </mat-option>
              }
            </mat-autocomplete>
          </mat-form-field>

          <mat-form-field appearance="outline" subscriptSizing="dynamic">
            <mat-label>Month</mat-label>
            <mat-select [(ngModel)]="month" (selectionChange)="loadData()">
              @for (m of months; track m.value) {
                <mat-option [value]="m.value">{{ m.label }}</mat-option>
              }
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline" subscriptSizing="dynamic">
            <mat-label>Year</mat-label>
            <mat-select [(ngModel)]="year" (selectionChange)="loadData()">
              @for (y of years; track y) {
                <mat-option [value]="y">{{ y }}</mat-option>
              }
            </mat-select>
          </mat-form-field>

          <button mat-raised-button color="primary" [disabled]="!selectedEmployee" (click)="loadData()">
            <mat-icon>refresh</mat-icon> Load
          </button>
        </mat-card-content>
      </mat-card>

      @if (data(); as d) {
        <mat-card class="summary-card">
          <mat-card-content class="summary-grid">
            <div class="stat">
              <span class="stat-value">{{ d.summary.totalPresentDays }}</span>
              <span class="stat-label">Present</span>
            </div>
            <div class="stat">
              <span class="stat-value">{{ d.summary.totalLateOccurrences }}</span>
              <span class="stat-label">Late</span>
            </div>
            <div class="stat">
              <span class="stat-value">{{ d.summary.totalAbsentDays }}</span>
              <span class="stat-label">Absent</span>
            </div>
            <div class="stat">
              <span class="stat-value">{{ d.summary.totalLeaveDays }}</span>
              <span class="stat-label">Leave</span>
            </div>
            <div class="stat">
              <span class="stat-value">{{ d.summary.totalOvertimeHours }}</span>
              <span class="stat-label">OT Hours</span>
            </div>
            <div class="stat">
              <span class="stat-value">{{ d.summary.totalWorkedMinutes }}</span>
              <span class="stat-label">Worked Min</span>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card>
          <mat-card-content>
            <table mat-table [dataSource]="d.days" class="full-width">
              <ng-container matColumnDef="date">
                <th mat-header-cell *matHeaderCellDef>Date</th>
                <td mat-cell *matCellDef="let day">{{ day.date }}</td>
              </ng-container>

              <ng-container matColumnDef="status">
                <th mat-header-cell *matHeaderCellDef>Status</th>
                <td mat-cell *matCellDef="let day">
                  <mat-chip [color]="statusColor(day.status)" [highlighted]="day.status !== 'OnTime'" selected>
                    {{ day.status }}
                  </mat-chip>
                </td>
              </ng-container>

              <ng-container matColumnDef="firstPunchIn">
                <th mat-header-cell *matHeaderCellDef>Punch In</th>
                <td mat-cell *matCellDef="let day">{{ day.firstPunchIn || '—' }}</td>
              </ng-container>

              <ng-container matColumnDef="lastPunchOut">
                <th mat-header-cell *matHeaderCellDef>Punch Out</th>
                <td mat-cell *matCellDef="let day">{{ day.lastPunchOut || '—' }}</td>
              </ng-container>

              <ng-container matColumnDef="netWorkedMinutes">
                <th mat-header-cell *matHeaderCellDef>Worked (min)</th>
                <td mat-cell *matCellDef="let day">{{ day.netWorkedMinutes }}</td>
              </ng-container>

              <ng-container matColumnDef="lateMinutes">
                <th mat-header-cell *matHeaderCellDef>Late (min)</th>
                <td mat-cell *matCellDef="let day">{{ day.lateMinutes || '—' }}</td>
              </ng-container>

              <ng-container matColumnDef="overtimeMinutes">
                <th mat-header-cell *matHeaderCellDef>OT (min)</th>
                <td mat-cell *matCellDef="let day">{{ day.overtimeMinutes || '—' }}</td>
              </ng-container>

              <ng-container matColumnDef="indicators">
                <th mat-header-cell *matHeaderCellDef>Flags</th>
                <td mat-cell *matCellDef="let day">
                  @if (day.isOnLeave) { <mat-icon matTooltip="On Leave">flight_takeoff</mat-icon> }
                  @if (day.isHoliday) { <mat-icon matTooltip="Holiday">celebration</mat-icon> }
                </td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="columns"></tr>
              <tr mat-row *matRowDef="let row; columns: columns;"></tr>
            </table>

            @if (d.days.length === 0) {
              <div class="empty-state">No attendance data for this period.</div>
            }
          </mat-card-content>
        </mat-card>
      }
    </div>
  `,
  styles: [`
    .page-container { padding: 1.5rem; }
    .page-header { margin-bottom: 1rem; }
    .page-header h1 { margin: 0; font-size: 1.5rem; font-weight: 400; }
    .filters-card { margin-bottom: 1rem; }
    .filters { display: flex; gap: 1rem; align-items: center; flex-wrap: wrap; }
    .employee-field { min-width: 280px; flex: 1; }
    .summary-card { margin-bottom: 1rem; }
    .summary-grid { display: flex; gap: 1.5rem; flex-wrap: wrap; }
    .stat { display: flex; flex-direction: column; align-items: center; min-width: 80px; }
    .stat-value { font-size: 1.5rem; font-weight: 500; }
    .stat-label { font-size: 0.75rem; color: var(--mat-sys-on-surface-variant, #666); text-transform: uppercase; }
    .full-width { width: 100%; }
    .empty-state { text-align: center; padding: 2rem; color: var(--mat-sys-on-surface-variant, #666); }
  `],
})
export class AttendanceViewerComponent implements OnInit {
  private attendanceService = inject(AttendanceService);
  private employeeService = inject(EmployeeService);

  data = signal<AttendanceViewerResult | null>(null);
  selectedEmployee: EmployeeDto | null = null;
  searchTerm = '';
  allEmployees = signal<EmployeeDto[]>([]);
  filteredEmployees = signal<EmployeeDto[]>([]);
  month = new Date().getMonth() + 1;
  year = new Date().getFullYear();

  months = Array.from({ length: 12 }, (_, i) => ({ value: i + 1, label: new Date(0, i).toLocaleString('en', { month: 'long' }) }));
  years = Array.from({ length: 6 }, (_, i) => this.year + i - 2);
  columns = ['date', 'status', 'firstPunchIn', 'lastPunchOut', 'netWorkedMinutes', 'lateMinutes', 'overtimeMinutes', 'indicators'];

  ngOnInit(): void {
    this.employeeService.getList({ pageIndex: 0, pageSize: 500 }).subscribe(r => {
      this.allEmployees.set(r.items);
      this.filterEmployees();
    });
  }

  filterEmployees(): void {
    const term = this.searchTerm.toLowerCase();
    this.filteredEmployees.set(
      this.allEmployees().filter(e =>
        e.fullName.toLowerCase().includes(term) ||
        e.employeeCode.toLowerCase().includes(term)
      )
    );
  }

  displayFn(e: EmployeeDto | null): string {
    return e ? `${e.fullName} (${e.employeeCode})` : '';
  }

  onEmployeeSelected(e: EmployeeDto): void {
    this.selectedEmployee = e;
    this.searchTerm = this.displayFn(e);
    this.loadData();
  }

  loadData(): void {
    if (!this.selectedEmployee) return;
    this.attendanceService.getViewer(this.selectedEmployee.id, this.year, this.month)
      .subscribe(r => this.data.set(r));
  }

  statusColor(status: string): string {
    return STATUS_COLORS[status] || '';
  }
}
