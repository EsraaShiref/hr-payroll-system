import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

@Component({
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Reject Payroll Run</h2>
    <mat-dialog-content>
      <p>Provide a reason for rejecting this payroll run. All calculations will be reset to Draft.</p>
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Reason</mat-label>
        <textarea matInput [(ngModel)]="reason" rows="3" required></textarea>
      </mat-form-field>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-raised-button color="warn" [disabled]="!reason" [mat-dialog-close]="reason">Reject</button>
    </mat-dialog-actions>
  `,
  styles: [`.full-width { width: 100%; margin-bottom: 0.5rem; }`],
})
export class RejectPayrollDialog {
  readonly dialogRef = inject(MatDialogRef<RejectPayrollDialog>);
  reason = '';
}

@Component({
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Patch Payroll Run</h2>
    <mat-dialog-content>
      <p>Select which employees to re-run payroll for. This creates a new payroll run for the same period.</p>
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Employee IDs (one per line)</mat-label>
        <textarea matInput [(ngModel)]="employeeIdsText" rows="5" placeholder="Paste employee IDs, one per line"></textarea>
      </mat-form-field>

      @if (skippedEmployeeIds.length > 0) {
        <p class="hint">Quick-add from skipped employees:</p>
        <div class="chip-group">
          @for (id of skippedEmployeeIds; track id) {
            <button mat-raised-button (click)="addEmployeeId(id)">+ employee</button>
          }
        </div>
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-raised-button color="primary" [disabled]="!parsedIds.length" [mat-dialog-close]="parsedIds">Create Patch Run</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width { width: 100%; margin-bottom: 0.5rem; }
    .hint { font-size: 0.875rem; color: var(--mat-sys-on-surface-variant, #666); margin: 0.5rem 0; }
    .chip-group { display: flex; gap: 0.5rem; flex-wrap: wrap; }
  `],
})
export class PatchPayrollDialog {
  readonly dialogRef = inject(MatDialogRef<PatchPayrollDialog>);
  employeeIdsText = '';
  skippedEmployeeIds: string[] = [];

  get parsedIds(): string[] {
    return this.employeeIdsText
      .split('\n')
      .map(s => s.trim())
      .filter(s => s.length > 0);
  }

  addEmployeeId(id: string): void {
    const existing = new Set(this.parsedIds);
    if (!existing.has(id)) {
      this.employeeIdsText += (this.employeeIdsText ? '\n' : '') + id;
    }
  }
}
