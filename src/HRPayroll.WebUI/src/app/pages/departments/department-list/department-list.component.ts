import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { DepartmentService } from '../../../core/services/department.service';
import { DepartmentDto } from '../../../models/hr';

@Component({
  selector: 'app-department-list',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatChipsModule,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Departments</h1>
      </div>

      @if (loading()) {
        <div class="spinner-container">
          <mat-progress-spinner mode="indeterminate" diameter="40" />
        </div>
      } @else {
        <mat-card appearance="outlined">
          <mat-card-content>
            <table mat-table [dataSource]="departments()">
              <ng-container matColumnDef="code">
                <th mat-header-cell *matHeaderCellDef>Code</th>
                <td mat-cell *matCellDef="let d">{{ d.code }}</td>
              </ng-container>
              <ng-container matColumnDef="name">
                <th mat-header-cell *matHeaderCellDef>Name</th>
                <td mat-cell *matCellDef="let d">{{ d.name }}</td>
              </ng-container>
              <ng-container matColumnDef="description">
                <th mat-header-cell *matHeaderCellDef>Description</th>
                <td mat-cell *matCellDef="let d">{{ d.description || '—' }}</td>
              </ng-container>
              <ng-container matColumnDef="managerName">
                <th mat-header-cell *matHeaderCellDef>Manager</th>
                <td mat-cell *matCellDef="let d">{{ d.managerName || '—' }}</td>
              </ng-container>
              <ng-container matColumnDef="isActive">
                <th mat-header-cell *matHeaderCellDef>Status</th>
                <td mat-cell *matCellDef="let d">
                  <span class="chip" [class.active]="d.isActive">
                    {{ d.isActive ? 'Active' : 'Inactive' }}
                  </span>
                </td>
              </ng-container>
              <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
            </table>
          </mat-card-content>
        </mat-card>
      }
    </div>
  `,
  styles: [`
    .page-container { padding: 1.5rem; max-width: 1200px; }
    .page-header { margin-bottom: 1rem; }
    .page-header h1 { margin: 0; font-size: 1.5rem; font-weight: 400; }
    .spinner-container { display: flex; justify-content: center; padding: 3rem; }
    table { width: 100%; }
    .chip { padding: 0.125rem 0.5rem; border-radius: 12px; font-size: 0.75rem; font-weight: 500; }
    .chip.active { background: #e8f5e9; color: #2e7d32; }
  `],
})
export class DepartmentListComponent implements OnInit {
  private departmentService = inject(DepartmentService);
  departments = signal<DepartmentDto[]>([]);
  loading = signal(true);
  displayedColumns = ['code', 'name', 'description', 'managerName', 'isActive'];

  ngOnInit(): void {
    this.departmentService.getList().subscribe({
      next: d => this.departments.set(d),
      error: () => this.loading.set(false),
      complete: () => this.loading.set(false),
    });
  }
}
