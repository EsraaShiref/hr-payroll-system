import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { provideNativeDateAdapter } from '@angular/material/core';
import { MatDialog } from '@angular/material/dialog';
import { AttendanceService } from '../../../core/services/attendance.service';
import { AttendanceExceptionDto } from '../../../models/attendance';
import { OverrideDialog } from './override-dialog.component';

@Component({
  selector: 'app-exceptions-report',
  standalone: true,
  providers: [provideNativeDateAdapter()],
  imports: [
    CommonModule, FormsModule, RouterModule,
    MatCardModule, MatTableModule, MatButtonModule, MatIconModule,
    MatChipsModule, MatProgressSpinnerModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatDatepickerModule,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Attendance Exceptions</h1>
      </div>

      <mat-card class="filters-card">
        <mat-card-content>
          <div class="filters-row">
            <mat-form-field appearance="outline" subscriptSizing="dynamic">
              <mat-label>Exception Type</mat-label>
              <mat-select [(ngModel)]="filterType" (selectionChange)="loadExceptions()">
                <mat-option value="">All</mat-option>
                <mat-option value="PendingReview">Pending Review</mat-option>
                <mat-option value="AbsentUnexcused">Unexcused Absence</mat-option>
                <mat-option value="LateArrival">Late Arrival</mat-option>
                <mat-option value="EarlyDeparture">Early Departure</mat-option>
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline" subscriptSizing="dynamic">
              <mat-label>From</mat-label>
              <input matInput [matDatepicker]="fromPicker" [(ngModel)]="fromDate" (dateChange)="loadExceptions()">
              <mat-datepicker-toggle matSuffix [for]="fromPicker"/>
              <mat-datepicker #fromPicker/>
            </mat-form-field>

            <mat-form-field appearance="outline" subscriptSizing="dynamic">
              <mat-label>To</mat-label>
              <input matInput [matDatepicker]="toPicker" [(ngModel)]="toDate" (dateChange)="loadExceptions()">
              <mat-datepicker-toggle matSuffix [for]="toPicker"/>
              <mat-datepicker #toPicker/>
            </mat-form-field>

            <button mat-raised-button color="primary" (click)="loadExceptions()">
              <mat-icon>search</mat-icon> Refresh
            </button>
          </div>
        </mat-card-content>
      </mat-card>

      @if (loading()) {
        <div class="spinner-container">
          <mat-progress-spinner mode="indeterminate" diameter="40" />
        </div>
      } @else if (exceptions().length === 0) {
        <mat-card><mat-card-content class="empty">No exceptions found for the selected filters.</mat-card-content></mat-card>
      } @else {
        <mat-card>
          <table mat-table [dataSource]="exceptions()" class="full-width">
            <ng-container matColumnDef="employee">
              <th mat-header-cell *matHeaderCellDef>Employee</th>
              <td mat-cell *matCellDef="let e">{{ e.employeeName }} ({{ e.employeeCode }})</td>
            </ng-container>

            <ng-container matColumnDef="date">
              <th mat-header-cell *matHeaderCellDef>Date</th>
              <td mat-cell *matCellDef="let e">{{ e.date }}</td>
            </ng-container>

            <ng-container matColumnDef="type">
              <th mat-header-cell *matHeaderCellDef>Type</th>
              <td mat-cell *matCellDef="let e">
                <mat-chip [class]="severityClass(e.severity)" [disabled]="true">{{ e.exceptionType }}</mat-chip>
              </td>
            </ng-container>

            <ng-container matColumnDef="details">
              <th mat-header-cell *matHeaderCellDef>Details</th>
              <td mat-cell *matCellDef="let e" class="details-cell">{{ e.details }}</td>
            </ng-container>

            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef></th>
              <td mat-cell *matCellDef="let e">
                @if (e.canOverride) {
                  <button mat-raised-button color="primary" (click)="openOverride(e)">
                    <mat-icon>edit</mat-icon> Override
                  </button>
                }
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
          </table>
        </mat-card>
      }
    </div>
  `,
  styles: [`
    .page-container { padding: 1.5rem; }
    .page-header { margin-bottom: 1rem; }
    .page-header h1 { margin: 0; font-size: 1.5rem; font-weight: 400; }
    .filters-card { margin-bottom: 1rem; }
    .filters-row { display: flex; gap: 1rem; align-items: center; flex-wrap: wrap; }
    .spinner-container { display: flex; justify-content: center; padding: 3rem; }
    .empty { text-align: center; color: #666; padding: 2rem; }
    .full-width { width: 100%; }
    .details-cell { max-width: 300px; white-space: normal; word-wrap: break-word; font-size: 0.85rem; }
    .mat-mdc-chip.mat-mdc-chip-disabled { opacity: 1; }
  `],
})
export class ExceptionsReportComponent implements OnInit {
  private attendanceService = inject(AttendanceService);
  private dialog = inject(MatDialog);

  displayedColumns = ['employee', 'date', 'type', 'details', 'actions'];
  exceptions = signal<AttendanceExceptionDto[]>([]);
  loading = signal(false);

  filterType = '';
  fromDate: Date | null = null;
  toDate: Date | null = null;

  ngOnInit(): void {
    const now = new Date();
    this.fromDate = new Date(now.getFullYear(), now.getMonth(), 1);
    this.toDate = now;
    this.loadExceptions();
  }

  loadExceptions(): void {
    this.loading.set(true);
    const params: Record<string, string> = {};
    if (this.filterType) params['exceptionType'] = this.filterType;
    if (this.fromDate) params['fromDate'] = this.fromDate.toISOString().split('T')[0];
    if (this.toDate) params['toDate'] = this.toDate.toISOString().split('T')[0];

    this.attendanceService.getExceptions(params).subscribe({
      next: result => this.exceptions.set(result),
      error: () => this.loading.set(false),
      complete: () => this.loading.set(false),
    });
  }

  openOverride(exception: AttendanceExceptionDto): void {
    const ref = this.dialog.open(OverrideDialog, {
      width: '450px',
      data: { exception },
    });
    ref.afterClosed().subscribe(result => {
      if (result) {
        this.attendanceService.overrideSummary(result).subscribe({
          next: () => this.loadExceptions(),
          error: () => {},
        });
      }
    });
  }

  severityClass(severity: string): string {
    return severity === 'Error' ? 'chip-error' : 'chip-warning';
  }
}
