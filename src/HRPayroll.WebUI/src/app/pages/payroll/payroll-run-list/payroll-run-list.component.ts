import { Component, OnInit, inject, signal, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatTableModule, MatTable } from '@angular/material/table';
import { MatSortModule, MatSort, Sort } from '@angular/material/sort';
import { MatPaginatorModule, MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { MatChipsModule } from '@angular/material/chips';
import { PayrollService } from '../../../core/services/payroll.service';
import { PayrollRunListItem, PaginatedPayrollRuns } from '../../../models/hr';
import { RunPayrollDialog } from '../run-payroll-dialog/run-payroll-dialog.component';

@Component({
  selector: 'app-payroll-run-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Payroll Runs</h1>
        <button mat-raised-button color="primary" (click)="openRunDialog()">
          <mat-icon>play_arrow</mat-icon>
          Run Payroll
        </button>
      </div>

      @if (loading()) {
        <div class="spinner-container">
          <mat-progress-spinner mode="indeterminate" diameter="40" />
        </div>
      }

      <div class="table-container" [class.hidden]="loading()">
        <table mat-table matSort (matSortChange)="onSort($event)" [dataSource]="data().items">
          <ng-container matColumnDef="period">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Period</th>
            <td mat-cell *matCellDef="let r">{{ monthNames[r.month - 1] }} {{ r.year }}</td>
          </ng-container>

          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let r">
              <mat-chip [class]="statusClass(r.status)" [disabled]="true">{{ r.status }}</mat-chip>
            </td>
          </ng-container>

          <ng-container matColumnDef="processedBy">
            <th mat-header-cell *matHeaderCellDef>Processed By</th>
            <td mat-cell *matCellDef="let r">{{ r.processedBy }}</td>
          </ng-container>

          <ng-container matColumnDef="startedAt">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Started</th>
            <td mat-cell *matCellDef="let r">{{ r.startedAt | date:'medium' }}</td>
          </ng-container>

          <ng-container matColumnDef="totalEmployees">
            <th mat-header-cell *matHeaderCellDef>Employees</th>
            <td mat-cell *matCellDef="let r">{{ r.totalEmployees }}</td>
          </ng-container>

          <ng-container matColumnDef="totalNetPay">
            <th mat-header-cell *matHeaderCellDef>Net Pay</th>
            <td mat-cell *matCellDef="let r">{{ r.totalNetPay | currency:'USD':'symbol':'1.2-2' }}</td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns" class="clickable-row" (click)="navigateTo(row.id)"></tr>
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
    .spinner-container { display: flex; justify-content: center; padding: 3rem; }
    .table-container { position: relative; }
    .table-container.hidden { display: none; }
    table { width: 100%; }
    .clickable-row { cursor: pointer; }
    .clickable-row:hover { background: var(--mat-sys-surface-container-high, #f5f5f5); }
  `],
})
export class PayrollRunListComponent implements OnInit {
  private payrollService = inject(PayrollService);
  private dialog = inject(MatDialog);
  private router = inject(Router);

  displayedColumns = ['period', 'status', 'processedBy', 'startedAt', 'totalEmployees', 'totalNetPay'];
  monthNames = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];

  data = signal<PaginatedPayrollRuns>({ items: [], totalCount: 0, page: 0, pageSize: 20 });
  loading = signal(false);

  pageIndex = 0;
  pageSize = 20;
  sortField = 'year';
  sortDirection = 'desc';

  ngOnInit(): void {
    this.loadData();
  }

  openRunDialog(): void {
    const dialogRef = this.dialog.open(RunPayrollDialog, { width: '400px' });
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.payrollService.run(result).subscribe({
          next: (id) => this.router.navigate(['/payroll', id]),
          error: () => {},
        });
      }
    });
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

  navigateTo(id: string): void {
    this.router.navigate(['/payroll', id]);
  }

  loadData(): void {
    this.loading.set(true);
    this.payrollService.getList(this.pageIndex + 1, this.pageSize).subscribe({
      next: result => this.data.set(result),
      error: () => this.loading.set(false),
      complete: () => this.loading.set(false),
    });
  }

  statusClass(status: string): string {
    switch (status) {
      case 'Draft': return 'status-draft';
      case 'Processing': return 'status-processing';
      case 'PendingReview': return 'status-pending';
      case 'Approved': return 'status-approved';
      case 'Finalized': return 'status-finalized';
      default: return '';
    }
  }
}
