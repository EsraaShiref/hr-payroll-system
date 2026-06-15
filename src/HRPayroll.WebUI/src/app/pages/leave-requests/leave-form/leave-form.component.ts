import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule, NgForm } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { provideNativeDateAdapter } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { LeaveRequestService, SubmitLeaveRequest } from '../../../core/services/leave-request.service';

@Component({
  selector: 'app-leave-form',
  standalone: true,
  providers: [provideNativeDateAdapter()],
  imports: [
    CommonModule, FormsModule,
    MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatDatepickerModule, MatButtonModule, MatIconModule, MatSnackBarModule,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <button mat-icon-button (click)="goBack()"><mat-icon>arrow_back</mat-icon></button>
        <h1>Submit Leave Request</h1>
      </div>

      <mat-card>
        <mat-card-content>
          <form #form="ngForm" (ngSubmit)="onSubmit(form)" autocomplete="off">
            <div class="form-grid">
              <mat-form-field appearance="outline">
                <mat-label>Leave Type</mat-label>
                <mat-select name="leaveType" [(ngModel)]="model.leaveType" required>
                  <mat-option value="Annual">Annual</mat-option>
                  <mat-option value="Sick">Sick</mat-option>
                  <mat-option value="Personal">Personal</mat-option>
                  <mat-option value="Maternity">Maternity</mat-option>
                  <mat-option value="Paternity">Paternity</mat-option>
                  <mat-option value="Bereavement">Bereavement</mat-option>
                  <mat-option value="Unpaid">Unpaid</mat-option>
                </mat-select>
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Start Date</mat-label>
                <input matInput [matDatepicker]="startPicker" name="startDate"
                       [(ngModel)]="model.startDate" required>
                <mat-datepicker-toggle matSuffix [for]="startPicker"/>
                <mat-datepicker #startPicker/>
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>End Date</mat-label>
                <input matInput [matDatepicker]="endPicker" name="endDate"
                       [(ngModel)]="model.endDate" required>
                <mat-datepicker-toggle matSuffix [for]="endPicker"/>
                <mat-datepicker #endPicker/>
              </mat-form-field>

              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Reason</mat-label>
                <textarea matInput name="reason" [(ngModel)]="model.reason" rows="3"></textarea>
              </mat-form-field>
            </div>

            @if (error()) {
              <div class="error-message">{{ error() }}</div>
            }

            <div class="form-actions">
              <button mat-button type="button" (click)="goBack()">Cancel</button>
              <button mat-raised-button color="primary" type="submit" [disabled]="form.invalid || submitting()">
                @if (submitting()) { Submitting... } @else { Submit Request }
              </button>
            </div>
          </form>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .page-container { max-width: 700px; margin: 0 auto; padding: 1.5rem; }
    .page-header { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1.5rem; }
    .page-header h1 { margin: 0; font-size: 1.5rem; font-weight: 500; }
    .form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
    .full-width { grid-column: 1 / -1; }
    .form-actions { display: flex; justify-content: flex-end; gap: 0.75rem; margin-top: 1.5rem; }
    .error-message { color: #d32f2f; background: #fce4ec; padding: 0.75rem; border-radius: 4px; margin-top: 1rem; font-size: 0.875rem; }
  `]
})
export class LeaveFormComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);
  private leaveService = inject(LeaveRequestService);

  submitting = signal(false);
  error = signal('');

  model: SubmitLeaveRequest = {
    employeeId: '',
    leaveType: 'Annual',
    startDate: '',
    endDate: '',
    reason: null,
  };

  ngOnInit(): void {
    const empId = this.route.snapshot.paramMap.get('employeeId');
    if (empId) this.model.employeeId = empId;
  }

  goBack(): void {
    this.router.navigate(['/employees', this.model.employeeId]);
  }

  onSubmit(form: NgForm): void {
    if (form.invalid) return;
    this.submitting.set(true);
    this.error.set('');

    this.leaveService.submit(this.model).subscribe({
      next: () => {
        this.snackBar.open('Leave request submitted', 'Close', { duration: 3000 });
        this.goBack();
      },
      error: err => {
        this.submitting.set(false);
        this.error.set(err.error?.detail || 'Failed to submit leave request.');
      },
    });
  }
}
