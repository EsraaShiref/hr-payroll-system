import { Component, signal, ViewChild, AfterViewInit, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatTableModule, MatTable } from '@angular/material/table';
import { MatSortModule, MatSort, Sort } from '@angular/material/sort';
import { MatPaginatorModule, MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { EmployeeService } from '../../../core/services/employee.service';
import { DepartmentService } from '../../../core/services/department.service';
import { EmployeeDto, DepartmentDto, PaginatedList } from '../../../models/hr';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';

@Component({
  selector: 'app-employee-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatButtonModule,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Employees</h1>
        <button mat-raised-button color="primary" routerLink="/employees/new">
          <mat-icon>add</mat-icon>
          New Employee
        </button>
      </div>

      <div class="filters-row">
        <mat-form-field appearance="outline" subscriptSizing="dynamic">
          <mat-label>Search</mat-label>
          <input
            matInput
            [(ngModel)]="searchTerm"
            (ngModelChange)="onSearchChange($event)"
            placeholder="Name or employee code"
          />
          <mat-icon matSuffix>search</mat-icon>
        </mat-form-field>

        <mat-form-field appearance="outline" subscriptSizing="dynamic">
          <mat-label>Department</mat-label>
          <mat-select [(ngModel)]="departmentFilter" (selectionChange)="loadData()">
            <mat-option [value]="null">All</mat-option>
            @for (dept of departments(); track dept.id) {
              <mat-option [value]="dept.id">{{ dept.name }}</mat-option>
            }
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline" subscriptSizing="dynamic">
          <mat-label>Status</mat-label>
          <mat-select [(ngModel)]="statusFilter" (selectionChange)="loadData()">
            <mat-option [value]="null">All</mat-option>
            <mat-option value="Active">Active</mat-option>
            <mat-option value="Terminated">Terminated</mat-option>
            <mat-option value="OnLeave">On Leave</mat-option>
          </mat-select>
        </mat-form-field>
      </div>

      @if (loading()) {
        <div class="spinner-container">
          <mat-progress-spinner mode="indeterminate" diameter="40" />
        </div>
      }

      <div class="table-container" [class.hidden]="loading()">
        <table mat-table matSort (matSortChange)="onSort($event)" [dataSource]="data().items">
          <ng-container matColumnDef="employeeCode">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Code</th>
            <td mat-cell *matCellDef="let e">{{ e.employeeCode }}</td>
          </ng-container>

          <ng-container matColumnDef="fullName">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Name</th>
            <td mat-cell *matCellDef="let e">
              <a [routerLink]="[e.id]">{{ e.fullName }}</a>
            </td>
          </ng-container>

          <ng-container matColumnDef="departmentName">
            <th mat-header-cell *matHeaderCellDef>Department</th>
            <td mat-cell *matCellDef="let e">{{ e.departmentName }}</td>
          </ng-container>

          <ng-container matColumnDef="positionTitle">
            <th mat-header-cell *matHeaderCellDef>Position</th>
            <td mat-cell *matCellDef="let e">{{ e.positionTitle }}</td>
          </ng-container>

          <ng-container matColumnDef="employmentStatus">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Status</th>
            <td mat-cell *matCellDef="let e">{{ e.employmentStatus }}</td>
          </ng-container>

          <ng-container matColumnDef="hireDate">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Hire Date</th>
            <td mat-cell *matCellDef="let e">{{ e.hireDate }}</td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
        </table>

        <mat-paginator
          [length]="data().totalCount"
          [pageSize]="pageSize"
          [pageIndex]="pageIndex"
          (page)="onPage($event)"
          [pageSizeOptions]="[10, 20, 50]"
          showFirstLastButtons
        />
      </div>
    </div>
  `,
  styles: [`
    .page-container { padding: 1.5rem; }
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; }
    .page-header h1 { margin: 0; font-size: 1.5rem; font-weight: 400; }
    .filters-row { display: flex; gap: 1rem; margin-bottom: 1rem; flex-wrap: wrap; }
    .filters-row mat-form-field { min-width: 200px; }
    .spinner-container { display: flex; justify-content: center; padding: 3rem; }
    .table-container { position: relative; }
    .table-container.hidden { display: none; }
    table { width: 100%; }
    a { color: var(--mat-sys-primary); text-decoration: none; font-weight: 500; }
    a:hover { text-decoration: underline; }
  `],
})
export class EmployeeListComponent implements OnInit {
  private employeeService = inject(EmployeeService);
  private departmentService = inject(DepartmentService);

  displayedColumns = ['employeeCode', 'fullName', 'departmentName', 'positionTitle', 'employmentStatus', 'hireDate'];

  data = signal<PaginatedList<EmployeeDto>>({
    items: [], pageIndex: 0, pageSize: 20, totalCount: 0, totalPages: 0, hasPreviousPage: false, hasNextPage: false,
  });
  departments = signal<DepartmentDto[]>([]);
  loading = signal(false);

  pageIndex = 0;
  pageSize = 20;
  sortField = 'lastName';
  sortDirection = 'asc';
  searchTerm = '';
  departmentFilter: string | null = null;
  statusFilter: string | null = null;

  private searchSubject = new Subject<string>();

  constructor() {
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged(),
    ).subscribe(() => this.loadData());
  }

  ngOnInit(): void {
    this.departmentService.getList().subscribe(d => this.departments.set(d));
    this.loadData();
  }

  onSearchChange(_value: string): void {
    this.searchSubject.next(this.searchTerm);
  }

  onSort(sort: Sort): void {
    if (sort.direction) {
      this.sortField = sort.active;
      this.sortDirection = sort.direction;
    }
    this.loadData();
  }

  onPage(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadData();
  }

  loadData(): void {
    this.loading.set(true);
    this.employeeService.getList({
      pageIndex: this.pageIndex,
      pageSize: this.pageSize,
      sortField: this.sortField,
      sortDirection: this.sortDirection,
      searchTerm: this.searchTerm || undefined,
      departmentId: this.departmentFilter ?? undefined,
      employmentStatus: this.statusFilter ?? undefined,
    }).subscribe({
      next: result => this.data.set(result),
      error: () => this.loading.set(false),
      complete: () => this.loading.set(false),
    });
  }
}
