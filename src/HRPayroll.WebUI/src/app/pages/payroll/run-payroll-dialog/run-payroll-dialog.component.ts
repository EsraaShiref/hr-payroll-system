import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';

@Component({
  standalone: true,
  imports: [
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
  ],
  template: `
    <h2 mat-dialog-title>Run Payroll</h2>
    <mat-dialog-content>
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Year</mat-label>
        <input matInput type="number" [(ngModel)]="year" min="2020" max="2100" required />
      </mat-form-field>

      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Month</mat-label>
        <mat-select [(ngModel)]="month" required>
          @for (m of months; track m.value) {
            <mat-option [value]="m.value">{{ m.label }}</mat-option>
          }
        </mat-select>
      </mat-form-field>

      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Notes (optional)</mat-label>
        <textarea matInput [(ngModel)]="notes" rows="3"></textarea>
      </mat-form-field>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-raised-button color="primary" [disabled]="!year || !month" [mat-dialog-close]="{ year, month, notes }">
        Start Run
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width { width: 100%; margin-bottom: 0.5rem; }
  `],
})
export class RunPayrollDialog {
  readonly dialogRef = inject(MatDialogRef<RunPayrollDialog>);

  year = new Date().getFullYear();
  month = new Date().getMonth() + 1;
  notes = '';

  months = [
    { value: 1, label: 'January' }, { value: 2, label: 'February' },
    { value: 3, label: 'March' }, { value: 4, label: 'April' },
    { value: 5, label: 'May' }, { value: 6, label: 'June' },
    { value: 7, label: 'July' }, { value: 8, label: 'August' },
    { value: 9, label: 'September' }, { value: 10, label: 'October' },
    { value: 11, label: 'November' }, { value: 12, label: 'December' },
  ];
}
