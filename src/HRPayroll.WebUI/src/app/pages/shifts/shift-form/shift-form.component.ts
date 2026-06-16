import { Component, inject, input, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { ShiftService, CreateShiftRequest } from '../../../core/services/shift-holiday.service';

interface DayCheckbox {
  label: string;
  value: number;
  checked: boolean;
}

@Component({
  selector: 'app-shift-form',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterModule,
    MatCardModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatCheckboxModule,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>{{ isEdit() ? 'Edit Shift' : 'New Shift' }}</h1>
      </div>

      <mat-card>
        <mat-card-content>
          <div class="form-grid">
            <mat-form-field appearance="outline">
              <mat-label>Name</mat-label>
              <input matInput [(ngModel)]="name" required />
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Description</mat-label>
              <input matInput [(ngModel)]="description" />
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Start Time</mat-label>
              <input matInput type="time" [(ngModel)]="startTime" required />
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>End Time</mat-label>
              <input matInput type="time" [(ngModel)]="endTime" required />
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Grace Period (min)</mat-label>
              <input matInput type="number" [(ngModel)]="gracePeriodMinutes" />
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Late Threshold (min)</mat-label>
              <input matInput type="number" [(ngModel)]="lateThresholdMinutes" />
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Early Departure Threshold (min)</mat-label>
              <input matInput type="number" [(ngModel)]="earlyDepartureThresholdMinutes" />
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Overtime Threshold (min)</mat-label>
              <input matInput type="number" [(ngModel)]="overtimeThresholdMinutes" />
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Min Work Minutes</mat-label>
              <input matInput type="number" [(ngModel)]="minimumWorkMinutes" />
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Max Break (min)</mat-label>
              <input matInput type="number" [(ngModel)]="maxBreakMinutes" />
            </mat-form-field>
          </div>

          <div class="working-days-section">
            <label class="section-label">Working Days</label>
            <div class="days-grid">
              @for (day of days; track day.value) {
                <mat-checkbox [(ngModel)]="day.checked">
                  {{ day.label }}
                </mat-checkbox>
              }
            </div>
          </div>
        </mat-card-content>
        <mat-card-actions align="end">
          <button mat-button routerLink="/shifts">Cancel</button>
          <button mat-raised-button color="primary" [disabled]="!name || !startTime || !endTime" (click)="save()">
            {{ isEdit() ? 'Update' : 'Create' }}
          </button>
        </mat-card-actions>
      </mat-card>
    </div>
  `,
  styles: [`
    .page-container { padding: 1.5rem; max-width: 800px; }
    .page-header { margin-bottom: 1rem; }
    .page-header h1 { margin: 0; font-size: 1.5rem; font-weight: 400; }
    .form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
    .working-days-section { margin-top: 1.5rem; }
    .section-label { display: block; font-size: 0.85rem; font-weight: 500; margin-bottom: 0.5rem; color: var(--mat-sys-on-surface-variant, #666); }
    .days-grid { display: flex; gap: 1rem; flex-wrap: wrap; }
  `],
})
export class ShiftFormComponent implements OnInit {
  private shiftService = inject(ShiftService);
  private router = inject(Router);

  shiftId = input<string>();
  isEdit = signal(false);

  name = '';
  description = '';
  startTime = '08:00';
  endTime = '17:00';
  gracePeriodMinutes = 0;
  lateThresholdMinutes = 15;
  earlyDepartureThresholdMinutes = 15;
  overtimeThresholdMinutes = 60;
  minimumWorkMinutes = 240;
  maxBreakMinutes = 60;

  days: DayCheckbox[] = [
    { label: 'Mon', value: 1, checked: true },
    { label: 'Tue', value: 2, checked: true },
    { label: 'Wed', value: 4, checked: true },
    { label: 'Thu', value: 8, checked: true },
    { label: 'Fri', value: 16, checked: true },
    { label: 'Sat', value: 32, checked: false },
    { label: 'Sun', value: 64, checked: false },
  ];

  ngOnInit(): void {
    if (this.shiftId()) {
      this.isEdit.set(true);
      this.shiftService.getById(this.shiftId()!).subscribe(s => {
        this.name = s.name;
        this.description = s.description ?? '';
        this.startTime = s.startTime;
        this.endTime = s.endTime;
        this.gracePeriodMinutes = s.gracePeriodMinutes;
        this.lateThresholdMinutes = s.lateThresholdMinutes;
        this.earlyDepartureThresholdMinutes = s.earlyDepartureThresholdMinutes;
        this.overtimeThresholdMinutes = s.overtimeThresholdMinutes;
        this.minimumWorkMinutes = s.minimumWorkMinutesForPresence;
        this.maxBreakMinutes = s.maxBreakMinutes;
        this.days.forEach(d => d.checked = (s.workingDays & d.value) !== 0);
      });
    }
  }

  get workingDaysValue(): number {
    return this.days.filter(d => d.checked).reduce((acc, d) => acc | d.value, 0);
  }

  save(): void {
    const request: CreateShiftRequest = {
      name: this.name,
      description: this.description || null,
      startTime: this.startTime,
      endTime: this.endTime,
      gracePeriodMinutes: this.gracePeriodMinutes,
      lateThresholdMinutes: this.lateThresholdMinutes,
      earlyDepartureThresholdMinutes: this.earlyDepartureThresholdMinutes,
      overtimeThresholdMinutes: this.overtimeThresholdMinutes,
      minimumWorkMinutesForPresence: this.minimumWorkMinutes,
      maxBreakMinutes: this.maxBreakMinutes,
      workingDays: this.workingDaysValue,
    };

    const nav = () => this.router.navigate(['/shifts']);
    if (this.isEdit()) {
      this.shiftService.update(this.shiftId()!, request).subscribe({ next: nav, error: () => {} });
    } else {
      this.shiftService.create(request).subscribe({ next: nav, error: () => {} });
    }
  }
}
