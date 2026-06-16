import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { SelfServiceService } from '../../../core/services/self-service.service';
import { MyLeaveRequestDto } from '../../../models/self-service';
import { LeaveBalanceDto } from '../../../models/attendance';

@Component({
  selector: 'app-my-leave',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterModule,
    MatCardModule, MatTableModule, MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule, MatSelectModule, MatChipsModule,
    MatDatepickerModule,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>My Leave</h1>
      </div>

      <mat-card class="balances-card">
        <mat-card-content class="balances">
          @for (b of balances(); track b.leaveType) {
            <div class="balance-item">
              <span class="balance-type">{{ b.leaveType }}</span>
              <span class="balance-remaining">{{ b.remainingDays }} / {{ b.totalDays }}</span>
            </div>
          }
        </mat-card-content>
      </mat-card>

      <mat-card class="form-card">
        <mat-card-content class="form-row">
          <mat-form-field appearance="outline" subscriptSizing="dynamic">
            <mat-label>Leave Type</mat-label>
            <mat-select [(ngModel)]="newLeave.leaveType">
              <mat-option value="Annual">Annual</mat-option>
              <mat-option value="Sick">Sick</mat-option>
              <mat-option value="Personal">Personal</mat-option>
              <mat-option value="Maternity">Maternity</mat-option>
              <mat-option value="Paternity">Paternity</mat-option>
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline" subscriptSizing="dynamic">
            <mat-label>Start</mat-label>
            <input matInput type="date" [(ngModel)]="newLeave.startDate" />
          </mat-form-field>
          <mat-form-field appearance="outline" subscriptSizing="dynamic">
            <mat-label>End</mat-label>
            <input matInput type="date" [(ngModel)]="newLeave.endDate" />
          </mat-form-field>
          <mat-form-field appearance="outline" subscriptSizing="dynamic" class="reason-field">
            <mat-label>Reason</mat-label>
            <input matInput [(ngModel)]="newLeave.reason" />
          </mat-form-field>
          <button mat-raised-button color="primary" [disabled]="!newLeave.leaveType || !newLeave.startDate || !newLeave.endDate" (click)="submitLeave()">
            Submit
          </button>
        </mat-card-content>
      </mat-card>

      <mat-card>
        <mat-card-content>
          <table mat-table [dataSource]="requests()" class="full-width">
            <ng-container matColumnDef="dates">
              <th mat-header-cell *matHeaderCellDef>Dates</th>
              <td mat-cell *matCellDef="let r">{{ r.startDate }} — {{ r.endDate }}</td>
            </ng-container>

            <ng-container matColumnDef="type">
              <th mat-header-cell *matHeaderCellDef>Type</th>
              <td mat-cell *matCellDef="let r">{{ r.leaveType }}</td>
            </ng-container>

            <ng-container matColumnDef="days">
              <th mat-header-cell *matHeaderCellDef>Days</th>
              <td mat-cell *matCellDef="let r">{{ r.totalDays }}</td>
            </ng-container>

            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef>Status</th>
              <td mat-cell *matCellDef="let r">
                <mat-chip [color]="r.status === 'Approved' ? 'primary' : r.status === 'Rejected' ? 'warn' : ''" selected>
                  {{ r.status }}
                </mat-chip>
              </td>
            </ng-container>

            <ng-container matColumnDef="reason">
              <th mat-header-cell *matHeaderCellDef>Reason</th>
              <td mat-cell *matCellDef="let r">{{ r.reason || '—' }}</td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="['dates', 'type', 'days', 'status', 'reason']"></tr>
            <tr mat-row *matRowDef="let row; columns: ['dates', 'type', 'days', 'status', 'reason'];"></tr>
          </table>

          @if (requests().length === 0) {
            <div class="empty">No leave requests submitted.</div>
          }
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .page-container { padding: 1.5rem; }
    .page-header { margin-bottom: 1rem; }
    .page-header h1 { margin: 0; font-size: 1.5rem; font-weight: 400; }
    .balances-card { margin-bottom: 1rem; }
    .balances { display: flex; gap: 1.5rem; flex-wrap: wrap; }
    .balance-item { display: flex; flex-direction: column; align-items: center; }
    .balance-type { font-size: 0.8rem; color: var(--mat-sys-on-surface-variant, #666); }
    .balance-remaining { font-size: 1.1rem; font-weight: 500; }
    .form-card { margin-bottom: 1rem; }
    .form-row { display: flex; gap: 1rem; flex-wrap: wrap; align-items: center; }
    .reason-field { min-width: 200px; flex: 1; }
    .full-width { width: 100%; }
    .empty { text-align: center; padding: 2rem; color: var(--mat-sys-on-surface-variant, #666); }
  `],
})
export class MyLeaveComponent implements OnInit {
  private service = inject(SelfServiceService);

  requests = signal<MyLeaveRequestDto[]>([]);
  balances = signal<LeaveBalanceDto[]>([]);
  newLeave = { leaveType: '', startDate: '', endDate: '', reason: '' };

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.service.getMyLeaveRequests().subscribe(r => this.requests.set(r));
    this.service.getMyLeaveBalances().subscribe(b => this.balances.set(b));
  }

  submitLeave(): void {
    this.service.submitLeave({
      leaveType: this.newLeave.leaveType,
      startDate: this.newLeave.startDate,
      endDate: this.newLeave.endDate,
      reason: this.newLeave.reason || null,
    }).subscribe({
      next: () => {
        this.newLeave = { leaveType: '', startDate: '', endDate: '', reason: '' };
        this.loadData();
      },
    });
  }
}
