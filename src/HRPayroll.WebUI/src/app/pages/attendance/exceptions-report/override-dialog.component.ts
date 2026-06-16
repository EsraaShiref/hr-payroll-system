import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { AttendanceExceptionDto, OverrideSummaryRequest } from '../../../models/attendance';

@Component({
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Override Attendance</h2>
    <mat-dialog-content>
      <p class="exception-info">
        <strong>{{ data.exception.employeeName }}</strong> &mdash; {{ data.exception.date }}<br/>
        <span class="detail">{{ data.exception.details }}</span>
      </p>

      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Override Punch In (HH:mm)</mat-label>
        <input matInput [(ngModel)]="overridePunchIn" placeholder="e.g. 08:00" />
      </mat-form-field>

      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Override Punch Out (HH:mm)</mat-label>
        <input matInput [(ngModel)]="overridePunchOut" placeholder="e.g. 17:00" />
      </mat-form-field>

      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Reason</mat-label>
        <textarea matInput [(ngModel)]="reason" rows="3" required placeholder="Explain why this override is needed"></textarea>
      </mat-form-field>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-raised-button color="primary" [disabled]="!reason" [mat-dialog-close]="buildRequest()">Apply Override</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width { width: 100%; margin-bottom: 0.5rem; }
    .exception-info { margin-bottom: 1rem; font-size: 0.9rem; }
    .detail { font-size: 0.85rem; color: var(--mat-sys-on-surface-variant, #666); }
  `],
})
export class OverrideDialog {
  readonly dialogRef = inject(MatDialogRef<OverrideDialog>);
  readonly data = inject<{ exception: AttendanceExceptionDto }>(MAT_DIALOG_DATA);

  overridePunchIn = '';
  overridePunchOut = '';
  reason = '';

  buildRequest(): OverrideSummaryRequest {
    return {
      summaryId: this.data.exception.summaryId!,
      overridePunchIn: this.overridePunchIn || null,
      overridePunchOut: this.overridePunchOut || null,
      reason: this.reason,
    };
  }
}
