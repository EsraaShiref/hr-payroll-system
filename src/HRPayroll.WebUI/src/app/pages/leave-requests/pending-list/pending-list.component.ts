import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { LeaveRequestService } from '../../../core/services/leave-request.service';
import { LeaveRequestDto } from '../../../models/attendance';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-pending-leave-list',
  standalone: true,
  imports: [
    CommonModule, RouterModule, FormsModule,
    MatCardModule, MatTableModule, MatButtonModule, MatIconModule,
    MatChipsModule, MatProgressSpinnerModule, MatSnackBarModule,
    MatTooltipModule, MatDialogModule, MatFormFieldModule, MatInputModule,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Pending Leave Requests</h1>
      </div>

      @if (loading()) {
        <div class="loading"><mat-spinner diameter="40"/></div>
      } @else if (requests().length === 0) {
        <mat-card><mat-card-content class="empty">No pending leave requests.</mat-card-content></mat-card>
      } @else {
        <mat-card>
          <table mat-table [dataSource]="requests()" class="full-width">
            <ng-container matColumnDef="employee">
              <th mat-header-cell *matHeaderCellDef>Employee</th>
              <td mat-cell *matCellDef="let r">{{ r.employeeName }}</td>
            </ng-container>
            <ng-container matColumnDef="leaveType">
              <th mat-header-cell *matHeaderCellDef>Type</th>
              <td mat-cell *matCellDef="let r">
                <span class="chip type-chip">{{ r.leaveType }}</span>
              </td>
            </ng-container>
            <ng-container matColumnDef="dates">
              <th mat-header-cell *matHeaderCellDef>Dates</th>
              <td mat-cell *matCellDef="let r">
                {{ r.startDate | date }} – {{ r.endDate | date }}
                <span class="days-badge">{{ r.totalDays }} day{{ r.totalDays > 1 ? 's' : '' }}</span>
              </td>
            </ng-container>
            <ng-container matColumnDef="reason">
              <th mat-header-cell *matHeaderCellDef>Reason</th>
              <td mat-cell *matCellDef="let r">{{ r.reason || '—' }}</td>
            </ng-container>
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef>Actions</th>
              <td mat-cell *matCellDef="let r">
                <button mat-raised-button color="primary" size="small"
                        (click)="approve(r)" matTooltip="Approve">
                  <mat-icon>check</mat-icon>
                </button>
                <button mat-raised-button color="warn" size="small"
                        (click)="reject(r)" matTooltip="Reject" class="ml-1">
                  <mat-icon>close</mat-icon>
                </button>
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
    .page-header { margin-bottom: 1.5rem; }
    .page-header h1 { margin: 0; font-size: 1.5rem; font-weight: 500; }
    .loading { display: flex; justify-content: center; padding: 3rem; }
    .empty { text-align: center; color: #666; padding: 2rem; }
    .full-width { width: 100%; }
    .chip { padding: 0.15rem 0.6rem; border-radius: 12px; font-size: 0.8rem; font-weight: 500; }
    .type-chip { background: #e8f5e9; color: #2e7d32; }
    .days-badge { background: #f3e5f5; color: #7b1fa2; padding: 0.1rem 0.5rem; border-radius: 8px; font-size: 0.75rem; margin-left: 0.5rem; }
    .ml-1 { margin-left: 0.5rem; }
    th.mat-mdc-header-cell { font-weight: 600; color: #333; }
  `]
})
export class PendingLeaveListComponent implements OnInit {
  private leaveService = inject(LeaveRequestService);
  private authService = inject(AuthService);
  private snackBar = inject(MatSnackBar);

  requests = signal<LeaveRequestDto[]>([]);
  loading = signal(true);
  displayedColumns = ['employee', 'leaveType', 'dates', 'reason', 'actions'];

  ngOnInit(): void {
    this.loadPending();
  }

  private loadPending(): void {
    this.leaveService.getPending().subscribe({
      next: r => this.requests.set(r),
      error: () => {},
      complete: () => this.loading.set(false),
    });
  }

  approve(r: LeaveRequestDto): void {
    const userId = this.authService.userId();
    if (!userId) return;
    this.leaveService.approve(r.id, userId).subscribe({
      next: () => {
        this.snackBar.open('Leave approved', 'Close', { duration: 2000 });
        this.requests.update(list => list.filter(x => x.id !== r.id));
      },
    });
  }

  reject(r: LeaveRequestDto): void {
    const reason = prompt('Rejection reason:');
    if (!reason) return;
    const userId = this.authService.userId();
    if (!userId) return;
    this.leaveService.reject(r.id, userId, reason).subscribe({
      next: () => {
        this.snackBar.open('Leave rejected', 'Close', { duration: 2000 });
        this.requests.update(list => list.filter(x => x.id !== r.id));
      },
    });
  }
}
