import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { SelfServiceService } from '../../../core/services/self-service.service';

@Component({
  selector: 'app-my-profile',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatCardModule, MatButtonModule, MatFormFieldModule, MatInputModule,
    MatDividerModule, MatSnackBarModule,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>My Profile</h1>
      </div>

      <mat-card>
        <mat-card-content>
          <h2>Contact Information</h2>
          <div class="form-grid">
            <mat-form-field appearance="outline">
              <mat-label>Phone</mat-label>
              <input matInput [(ngModel)]="phoneNumber" />
            </mat-form-field>
          </div>

          <h3>Address</h3>
          <div class="form-grid">
            <mat-form-field appearance="outline">
              <mat-label>Street</mat-label>
              <input matInput [(ngModel)]="street" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>City</mat-label>
              <input matInput [(ngModel)]="city" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>State</mat-label>
              <input matInput [(ngModel)]="state" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Postal Code</mat-label>
              <input matInput [(ngModel)]="postalCode" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Country</mat-label>
              <input matInput [(ngModel)]="country" />
            </mat-form-field>
          </div>

          <h3>Emergency Contact</h3>
          <div class="form-grid">
            <mat-form-field appearance="outline">
              <mat-label>Contact Name</mat-label>
              <input matInput [(ngModel)]="emergencyName" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Contact Phone</mat-label>
              <input matInput [(ngModel)]="emergencyPhone" />
            </mat-form-field>
          </div>

          <div class="form-actions">
            <button mat-raised-button color="primary" (click)="saveProfile()">Save Changes</button>
          </div>
        </mat-card-content>
      </mat-card>

      <mat-card class="email-card">
        <mat-card-content>
          <h2>Email Address</h2>
          <p class="hint">Changing your email requires HR approval. A request will be created for review.</p>
          <div class="form-inline">
            <mat-form-field appearance="outline" class="email-field">
              <mat-label>New Email</mat-label>
              <input matInput type="email" [(ngModel)]="newEmail" />
            </mat-form-field>
            <button mat-raised-button color="primary" [disabled]="!newEmail" (click)="requestEmailChange()">
              Request Change
            </button>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .page-container { padding: 1.5rem; max-width: 700px; }
    .page-header { margin-bottom: 1rem; }
    .page-header h1 { margin: 0; font-size: 1.5rem; font-weight: 400; }
    h2 { font-size: 1.1rem; font-weight: 500; margin: 0 0 0.75rem; }
    h3 { font-size: 0.95rem; font-weight: 500; margin: 1rem 0 0.5rem; color: var(--mat-sys-on-surface-variant, #666); }
    .form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; }
    .form-actions { margin-top: 1rem; display: flex; justify-content: flex-end; }
    .email-card { margin-top: 1rem; }
    .hint { font-size: 0.85rem; color: var(--mat-sys-on-surface-variant, #666); margin-bottom: 0.75rem; }
    .form-inline { display: flex; gap: 0.75rem; align-items: center; }
    .email-field { flex: 1; }
  `],
})
export class MyProfileComponent {
  private service = inject(SelfServiceService);
  private snackbar = inject(MatSnackBar);

  phoneNumber = '';
  street = '';
  city = '';
  state = '';
  postalCode = '';
  country = '';
  emergencyName = '';
  emergencyPhone = '';
  newEmail = '';

  saveProfile(): void {
    this.service.updateProfile({
      phoneNumber: this.phoneNumber || null,
      street: this.street || null,
      city: this.city || null,
      state: this.state || null,
      postalCode: this.postalCode || null,
      country: this.country || null,
      emergencyContactName: this.emergencyName || null,
      emergencyContactPhone: this.emergencyPhone || null,
    }).subscribe({
      next: () => this.snackbar.open('Profile updated', 'OK', { duration: 3000 }),
      error: () => this.snackbar.open('Failed to update profile', 'OK', { duration: 3000 }),
    });
  }

  requestEmailChange(): void {
    this.service.requestEmailChange({ newEmail: this.newEmail }).subscribe({
      next: () => {
        this.snackbar.open('Email change request submitted for HR approval', 'OK', { duration: 5000 });
        this.newEmail = '';
      },
      error: () => this.snackbar.open('Failed to submit email change', 'OK', { duration: 3000 }),
    });
  }
}
