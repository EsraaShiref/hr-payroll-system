import { Component, inject, model } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { provideNativeDateAdapter } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';

export interface TerminateDialogData {
  employeeName: string;
}

@Component({
  standalone: true,
  providers: [provideNativeDateAdapter()],
  imports: [
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatButtonModule,
  ],
  template: `
    <h2 mat-dialog-title>Terminate Employee</h2>
    <mat-dialog-content>
      <p>Terminate <strong>{{ data.employeeName }}</strong>?</p>
      <p class="warning">This action will end the active contract and set the employee status to Terminated.</p>

      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Termination Date</mat-label>
        <input matInput [matDatepicker]="picker" [(ngModel)]="terminationDate" required />
        <mat-datepicker-toggle matSuffix [for]="picker" />
        <mat-datepicker #picker />
      </mat-form-field>

      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Reason</mat-label>
        <textarea matInput [(ngModel)]="reason" rows="3" placeholder="e.g., Resignation, Retirement, Redundancy"></textarea>
      </mat-form-field>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-raised-button color="warn" [disabled]="!terminationDate" [mat-dialog-close]="{ terminationDate, reason }">
        Confirm Termination
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .warning { color: #dc2626; font-size: 0.875rem; background: #fef2f2; padding: 0.5rem 0.75rem; border-radius: 6px; margin: 0.5rem 0 1rem; }
    .full-width { width: 100%; margin-bottom: 0.5rem; }
  `],
})
export class TerminateDialog {
  readonly dialogRef = inject(MatDialogRef<TerminateDialog>);
  readonly data = inject<TerminateDialogData>(MAT_DIALOG_DATA);

  terminationDate: Date | null = null;
  reason = '';
}
