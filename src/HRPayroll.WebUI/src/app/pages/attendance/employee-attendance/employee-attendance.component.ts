import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { provideNativeDateAdapter } from '@angular/material/core';
import { FormsModule } from '@angular/forms';
import { AttendanceService } from '../../../core/services/attendance.service';
import { AttendanceRecordDto } from '../../../models/attendance';

@Component({
  selector: 'app-employee-attendance',
  standalone: true,
  providers: [provideNativeDateAdapter()],
  imports: [
    CommonModule, RouterModule, FormsModule,
    MatCardModule, MatTableModule, MatButtonModule, MatIconModule,
    MatChipsModule, MatProgressSpinnerModule, MatFormFieldModule,
    MatInputModule, MatDatepickerModule,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <button mat-icon-button routerLink="/employees/{{ employeeId }}"><mat-icon>arrow_back</mat-icon></button>
        <h1>Attendance Records</h1>
      </div>

      <mat-card class="date-range-card">
        <mat-card-content>
          <div class="date-range">
            <mat-form-field appearance="outline" subscriptSizing="dynamic">
              <mat-label>From</mat-label>
              <input matInput [matDatepicker]="fromPicker" [(ngModel)]="fromDate">
              <mat-datepicker-toggle matSuffix [for]="fromPicker"/>
              <mat-datepicker #fromPicker/>
            </mat-form-field>
            <mat-form-field appearance="outline" subscriptSizing="dynamic">
              <mat-label>To</mat-label>
              <input matInput [matDatepicker]="toPicker" [(ngModel)]="toDate">
              <mat-datepicker-toggle matSuffix [for]="toPicker"/>
              <mat-datepicker #toPicker/>
            </mat-form-field>
            <button mat-raised-button color="primary" (click)="loadRecords()">Search</button>
          </div>
        </mat-card-content>
      </mat-card>

      @if (loading()) {
        <div class="loading"><mat-spinner diameter="40"/></div>
      } @else if (records().length === 0) {
        <mat-card><mat-card-content class="empty">No attendance records found.</mat-card-content></mat-card>
      } @else {
        <mat-card>
          <table mat-table [dataSource]="records()" class="full-width">
            <ng-container matColumnDef="date">
              <th mat-header-cell *matHeaderCellDef>Date</th>
              <td mat-cell *matCellDef="let r">{{ r.date | date }}</td>
            </ng-container>
            <ng-container matColumnDef="clockIn">
              <th mat-header-cell *matHeaderCellDef>Clock In</th>
              <td mat-cell *matCellDef="let r">{{ r.clockIn || '—' }}</td>
            </ng-container>
            <ng-container matColumnDef="clockOut">
              <th mat-header-cell *matHeaderCellDef>Clock Out</th>
              <td mat-cell *matCellDef="let r">{{ r.clockOut || '—' }}</td>
            </ng-container>
            <ng-container matColumnDef="worked">
              <th mat-header-cell *matHeaderCellDef>Worked (min)</th>
              <td mat-cell *matCellDef="let r">{{ r.workedMinutes }}</td>
            </ng-container>
            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef>Status</th>
              <td mat-cell *matCellDef="let r">
                <span class="chip" [class.present]="r.status === 'Present'"
                      [class.late]="r.status === 'Late'"
                      [class.absent]="r.status === 'Absent'"
                      [class.half]="r.status === 'HalfDay'">
                  {{ r.status }}
                </span>
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
          </table>
        </mat-card>
      }
    </div>
  `,
  styles: [`
    .page-container { max-width: 1000px; margin: 0 auto; padding: 1.5rem; }
    .page-header { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1.5rem; }
    .page-header h1 { margin: 0; font-size: 1.5rem; font-weight: 500; }
    .date-range-card { margin-bottom: 1rem; }
    .date-range { display: flex; align-items: center; gap: 1rem; }
    .loading { display: flex; justify-content: center; padding: 3rem; }
    .empty { text-align: center; color: #666; padding: 2rem; }
    .full-width { width: 100%; }
    .chip { padding: 0.15rem 0.6rem; border-radius: 12px; font-size: 0.8rem; font-weight: 500; }
    .present { background: #e8f5e9; color: #2e7d32; }
    .late { background: #fff3e0; color: #e65100; }
    .absent { background: #fce4ec; color: #c62828; }
    .half { background: #f3e5f5; color: #7b1fa2; }
    th.mat-mdc-header-cell { font-weight: 600; color: #333; }
  `]
})
export class EmployeeAttendanceComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private attendanceService = inject(AttendanceService);

  employeeId = '';
  records = signal<AttendanceRecordDto[]>([]);
  loading = signal(true);
  fromDate = '';
  toDate = '';
  displayedColumns = ['date', 'clockIn', 'clockOut', 'worked', 'status'];

  ngOnInit(): void {
    this.employeeId = this.route.snapshot.paramMap.get('employeeId')!;
    const now = new Date();
    const first = new Date(now.getFullYear(), now.getMonth(), 1);
    this.fromDate = first.toISOString().split('T')[0];
    this.toDate = now.toISOString().split('T')[0];
    this.loadRecords();
  }

  loadRecords(): void {
    this.loading.set(true);
    this.attendanceService.getByEmployee(this.employeeId, this.fromDate, this.toDate).subscribe({
      next: r => this.records.set(r),
      error: () => {},
      complete: () => this.loading.set(false),
    });
  }
}
