import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { EmployeeService } from '../../../core/services/employee.service';
import { TerminateDialog } from '../terminate-dialog/terminate-dialog.component';
import { EmployeeDetailDto } from '../../../models/hr';

@Component({
  selector: 'app-employee-detail',
  standalone: true,
  imports: [
    CommonModule,
    DecimalPipe,
    RouterModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatTooltipModule,
    MatDialogModule,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <button mat-icon-button routerLink="/employees">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <h1>{{ employee()?.fullName }}</h1>
        <span class="spacer"></span>
        <span class="chip" [class.active]="employee()?.employmentStatus === 'Active'"
              [class.terminated]="employee()?.employmentStatus === 'Terminated'">
          {{ employee()?.employmentStatus }}
        </span>
        @if (employee()?.employmentStatus === 'Active') {
          <button mat-raised-button color="warn" (click)="openTerminateDialog()" matTooltip="Terminate employee">
            <mat-icon>person_remove</mat-icon>
            Terminate
          </button>
        }
      </div>

      @if (loading()) {
        <div class="spinner-container">
          <mat-progress-spinner mode="indeterminate" diameter="40" />
        </div>
      } @else if (employee(); as e) {
        <div class="detail-grid">
          <mat-card appearance="outlined">
            <mat-card-header><mat-card-title>Personal Information</mat-card-title></mat-card-header>
            <mat-card-content>
              <div class="field"><label>Employee Code</label><span>{{ e.employeeCode }}</span></div>
              <div class="field"><label>First Name</label><span>{{ e.firstName }}</span></div>
              <div class="field"><label>Middle Name</label><span>{{ e.middleName || '—' }}</span></div>
              <div class="field"><label>Last Name</label><span>{{ e.lastName }}</span></div>
              <div class="field"><label>Date of Birth</label><span>{{ e.dateOfBirth }}</span></div>
              <div class="field"><label>Gender</label><span>{{ e.gender }}</span></div>
              <div class="field"><label>National ID</label><span>{{ e.nationalId }}</span></div>
            </mat-card-content>
          </mat-card>

          <mat-card appearance="outlined">
            <mat-card-header><mat-card-title>Contact</mat-card-title></mat-card-header>
            <mat-card-content>
              <div class="field"><label>Email</label><span>{{ e.personalEmail || '—' }}</span></div>
              <div class="field"><label>Phone</label><span>{{ e.phoneNumber || '—' }}</span></div>
              <div class="field"><label>Address</label><span>{{ [e.street, e.city, e.state, e.postalCode, e.country].filter(x => x).join(', ') || '—' }}</span></div>
              <div class="field"><label>Emergency Contact</label><span>{{ e.emergencyContactName || '—' }}</span></div>
              <div class="field"><label>Emergency Phone</label><span>{{ e.emergencyContactPhone || '—' }}</span></div>
            </mat-card-content>
          </mat-card>

          <mat-card appearance="outlined">
            <mat-card-header><mat-card-title>Employment</mat-card-title></mat-card-header>
            <mat-card-content>
              <div class="field"><label>Department</label><span>{{ e.departmentName || '—' }}</span></div>
              <div class="field"><label>Position</label><span>{{ e.positionTitle || '—' }}</span></div>
              <div class="field"><label>Hire Date</label><span>{{ e.hireDate }}</span></div>
              <div class="field"><label>Termination Date</label><span>{{ e.terminationDate || '—' }}</span></div>
            </mat-card-content>
          </mat-card>

          @if (e.activeContract; as c) {
            <mat-card appearance="outlined">
              <mat-card-header>
                <mat-card-title>Active Contract</mat-card-title>
                <a mat-icon-button [routerLink]="['/contracts', c.id]" matTooltip="View contract">
                  <mat-icon>open_in_new</mat-icon>
                </a>
              </mat-card-header>
              <mat-card-content>
                <div class="field"><label>Type</label><span>{{ c.contractType }}</span></div>
                <div class="field"><label>Signed Date</label><span>{{ c.signedDate }}</span></div>
                <div class="field"><label>Base Salary</label><span>{{ c.currentVersion.baseSalaryAmount | number:'1.2-2' }} {{ c.currentVersion.baseSalaryCurrency }}</span></div>
                <div class="field"><label>Version</label><span>v{{ c.currentVersion.versionNumber }}</span></div>
                <div class="field"><label>Effective From</label><span>{{ c.currentVersion.effectiveFrom }}</span></div>
              </mat-card-content>
            </mat-card>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .page-container { padding: 1.5rem; max-width: 1200px; }
    .page-header { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1.5rem; }
    .page-header h1 { margin: 0; font-size: 1.5rem; font-weight: 400; }
    .spacer { flex: 1; }
    .chip { padding: 0.25rem 0.75rem; border-radius: 16px; font-size: 0.8rem; font-weight: 500; }
    .chip.active { background: #e8f5e9; color: #2e7d32; }
    .chip.terminated { background: #fbe9e7; color: #c62828; }
    .spinner-container { display: flex; justify-content: center; padding: 3rem; }
    .detail-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(350px, 1fr)); gap: 1rem; }
    .field { display: flex; flex-direction: column; margin-bottom: 0.75rem; }
    .field label { font-size: 0.75rem; color: var(--mat-sys-on-surface-variant); text-transform: uppercase; letter-spacing: 0.5px; }
    .field span { font-size: 0.9rem; margin-top: 0.125rem; }
    mat-card-header { display: flex; align-items: center; }
    mat-card-header a { margin-left: auto; }
  `],
})
export class EmployeeDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private dialog = inject(MatDialog);
  private employeeService = inject(EmployeeService);

  employee = signal<EmployeeDetailDto | null>(null);
  loading = signal(true);

  ngOnInit(): void {
    this.loadEmployee();
  }

  private loadEmployee(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.employeeService.getById(id).subscribe({
      next: e => this.employee.set(e),
      error: () => this.loading.set(false),
      complete: () => this.loading.set(false),
    });
  }

  openTerminateDialog(): void {
    const emp = this.employee();
    if (!emp) return;

    const dialogRef = this.dialog.open(TerminateDialog, {
      width: '450px',
      data: { employeeName: emp.fullName },
    });

    dialogRef.afterClosed().subscribe(result => {
      if (!result) return;
      this.employeeService.terminate(emp.id, result.terminationDate, result.reason).subscribe({
        next: () => this.loadEmployee(),
        error: () => {},
      });
    });
  }
}
