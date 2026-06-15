import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule, NgForm } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { provideNativeDateAdapter } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { EmployeeService, CreateEmployeeRequest } from '../../../core/services/employee.service';
import { DepartmentService } from '../../../core/services/department.service';
import { PositionService } from '../../../core/services/position.service';
import { DepartmentDto, PositionDto } from '../../../models/hr';

@Component({
  selector: 'app-employee-form',
  standalone: true,
  providers: [provideNativeDateAdapter()],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <button mat-icon-button routerLink="/employees"><mat-icon>arrow_back</mat-icon></button>
        <h1>New Employee</h1>
      </div>

      <mat-card appearance="outlined" class="form-card">
        <mat-card-content>
          <form #f="ngForm" (ngSubmit)="onSubmit(f)">
            <div class="form-row">
              <mat-form-field appearance="outline">
                <mat-label>Employee Code</mat-label>
                <input matInput name="employeeCode" [(ngModel)]="model.employeeCode" required maxlength="20" />
              </mat-form-field>
              <mat-form-field appearance="outline">
                <mat-label>National ID</mat-label>
                <input matInput name="nationalId" [(ngModel)]="model.nationalId" required />
              </mat-form-field>
            </div>

            <div class="form-row three">
              <mat-form-field appearance="outline">
                <mat-label>First Name</mat-label>
                <input matInput name="firstName" [(ngModel)]="model.firstName" required />
              </mat-form-field>
              <mat-form-field appearance="outline">
                <mat-label>Middle Name</mat-label>
                <input matInput name="middleName" [(ngModel)]="model.middleName" />
              </mat-form-field>
              <mat-form-field appearance="outline">
                <mat-label>Last Name</mat-label>
                <input matInput name="lastName" [(ngModel)]="model.lastName" required />
              </mat-form-field>
            </div>

            <div class="form-row three">
              <mat-form-field appearance="outline">
                <mat-label>Date of Birth</mat-label>
                <input matInput [matDatepicker]="dobPicker" name="dateOfBirth" [(ngModel)]="model.dateOfBirth" required />
                <mat-datepicker-toggle matSuffix [for]="dobPicker" />
                <mat-datepicker #dobPicker />
              </mat-form-field>
              <mat-form-field appearance="outline">
                <mat-label>Gender</mat-label>
                <mat-select name="gender" [(ngModel)]="model.gender" required>
                  <mat-option value="Male">Male</mat-option>
                  <mat-option value="Female">Female</mat-option>
                </mat-select>
              </mat-form-field>
              <mat-form-field appearance="outline">
                <mat-label>Hire Date</mat-label>
                <input matInput [matDatepicker]="hirePicker" name="hireDate" [(ngModel)]="model.hireDate" required />
                <mat-datepicker-toggle matSuffix [for]="hirePicker" />
                <mat-datepicker #hirePicker />
              </mat-form-field>
            </div>

            <div class="form-row">
              <mat-form-field appearance="outline">
                <mat-label>Department</mat-label>
                <mat-select name="departmentId" [(ngModel)]="model.departmentId" required (selectionChange)="onDepartmentChange()">
                  @for (d of departments(); track d.id) {
                    <mat-option [value]="d.id">{{ d.name }}</mat-option>
                  }
                </mat-select>
              </mat-form-field>
              <mat-form-field appearance="outline">
                <mat-label>Position</mat-label>
                <mat-select name="positionId" [(ngModel)]="model.positionId" required>
                  @for (p of filteredPositions(); track p.id) {
                    <mat-option [value]="p.id">{{ p.title }}</mat-option>
                  }
                </mat-select>
              </mat-form-field>
            </div>

            @if (error()) {
              <div class="error-banner">{{ error() }}</div>
            }

            <div class="actions">
              <button mat-stroked-button type="button" routerLink="/employees">Cancel</button>
              <button mat-raised-button color="primary" type="submit" [disabled]="f.invalid || submitting()">
                {{ submitting() ? 'Creating...' : 'Create Employee' }}
              </button>
            </div>
          </form>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .page-container { padding: 1.5rem; max-width: 800px; }
    .page-header { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1.5rem; }
    .page-header h1 { margin: 0; font-size: 1.5rem; font-weight: 400; }
    .form-card { padding: 1rem; }
    .form-row { display: flex; gap: 1rem; margin-bottom: 0.5rem; }
    .form-row mat-form-field { flex: 1; }
    .form-row.three mat-form-field { flex: 1; }
    .error-banner { background: #fef2f2; border: 1px solid #fecaca; border-radius: 6px; padding: 0.75rem; margin: 1rem 0; color: #dc2626; font-size: 0.875rem; }
    .actions { display: flex; justify-content: flex-end; gap: 0.75rem; margin-top: 1.5rem; }
  `],
})
export class EmployeeFormComponent implements OnInit {
  private router = inject(Router);
  private employeeService = inject(EmployeeService);
  private departmentService = inject(DepartmentService);
  private positionService = inject(PositionService);

  departments = signal<DepartmentDto[]>([]);
  allPositions = signal<PositionDto[]>([]);
  filteredPositions = signal<PositionDto[]>([]);
  submitting = signal(false);
  error = signal('');

  model: CreateEmployeeRequest = {
    employeeCode: '', firstName: '', middleName: null, lastName: '',
    dateOfBirth: '', gender: '', nationalId: '', departmentId: '', positionId: '', hireDate: '',
  };

  ngOnInit(): void {
    this.departmentService.getList().subscribe(d => this.departments.set(d));
    this.positionService.getList().subscribe(p => this.allPositions.set(p));
  }

  onDepartmentChange(): void {
    this.model.positionId = '';
    this.filteredPositions.set(
      this.allPositions().filter(p => p.departmentId === this.model.departmentId),
    );
  }

  private formatDate(value: string | Date): string {
    if (!value) return '';
    const d = typeof value === 'string' ? new Date(value) : value;
    return d.toISOString().split('T')[0];
  }

  onSubmit(f: NgForm): void {
    if (f.invalid) return;
    this.submitting.set(true);
    this.error.set('');

    const payload = {
      ...this.model,
      dateOfBirth: this.formatDate(this.model.dateOfBirth),
      hireDate: this.formatDate(this.model.hireDate),
    };

    this.employeeService.create(payload).subscribe({
      next: id => this.router.navigate(['/employees', id]),
      error: err => {
        this.submitting.set(false);
        this.error.set(err.error?.detail || 'Failed to create employee.');
      },
    });
  }
}
