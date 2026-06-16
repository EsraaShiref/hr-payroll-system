import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSelectModule } from '@angular/material/select';
import { HolidayService, HolidayDto } from '../../../core/services/shift-holiday.service';

@Component({
  selector: 'app-holiday-list',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatCardModule, MatTableModule, MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule, MatCheckboxModule, MatSelectModule,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Holidays</h1>
        <button mat-raised-button color="primary" (click)="showForm.set(true)" [disabled]="showForm()">
          <mat-icon>add</mat-icon> Add Holiday
        </button>
      </div>

      @if (showForm()) {
        <mat-card class="add-card">
          <mat-card-content class="add-form">
            <mat-form-field appearance="outline" subscriptSizing="dynamic">
              <mat-label>Holiday Name</mat-label>
              <input matInput [(ngModel)]="newName" />
            </mat-form-field>
            <mat-form-field appearance="outline" subscriptSizing="dynamic">
              <mat-label>Date</mat-label>
              <input matInput type="date" [(ngModel)]="newDate" />
            </mat-form-field>
            <mat-checkbox [(ngModel)]="newRecurring">Recurring yearly</mat-checkbox>
            <div class="form-actions">
              <button mat-button (click)="cancelAdd()">Cancel</button>
              <button mat-raised-button color="primary" [disabled]="!newName || !newDate" (click)="addHoliday()">
                Save
              </button>
            </div>
          </mat-card-content>
        </mat-card>
      }

      <mat-card>
        <mat-card-content>
          <div class="filter-bar">
            <mat-form-field appearance="outline" subscriptSizing="dynamic">
              <mat-label>Year</mat-label>
              <mat-select [(ngModel)]="selectedYear" (selectionChange)="loadHolidays()">
                @for (y of years; track y) {
                  <mat-option [value]="y">{{ y }}</mat-option>
                }
                <mat-option [value]="0">All</mat-option>
              </mat-select>
            </mat-form-field>
          </div>

          <table mat-table [dataSource]="holidays()" class="full-width">
            <ng-container matColumnDef="name">
              <th mat-header-cell *matHeaderCellDef>Holiday</th>
              <td mat-cell *matCellDef="let h">{{ h.name }}</td>
            </ng-container>

            <ng-container matColumnDef="date">
              <th mat-header-cell *matHeaderCellDef>Date</th>
              <td mat-cell *matCellDef="let h">{{ h.date }}</td>
            </ng-container>

            <ng-container matColumnDef="recurring">
              <th mat-header-cell *matHeaderCellDef>Recurring</th>
              <td mat-cell *matCellDef="let h">
                @if (h.isRecurringYearly) {
                  <mat-icon>repeat</mat-icon>
                }
              </td>
            </ng-container>

            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef></th>
              <td mat-cell *matCellDef="let h">
                <button mat-icon-button (click)="deleteHoliday(h.id)" matTooltip="Delete">
                  <mat-icon>delete</mat-icon>
                </button>
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="columns"></tr>
            <tr mat-row *matRowDef="let row; columns: columns;"></tr>
          </table>

          @if (holidays().length === 0) {
            <div class="empty-state">No holidays for this period.</div>
          }
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .page-container { padding: 1.5rem; }
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; }
    .page-header h1 { margin: 0; font-size: 1.5rem; font-weight: 400; }
    .add-card { margin-bottom: 1rem; }
    .add-form { display: flex; gap: 1rem; align-items: center; flex-wrap: wrap; }
    .form-actions { display: flex; gap: 0.5rem; }
    .filter-bar { margin-bottom: 0.5rem; }
    .full-width { width: 100%; }
    .empty-state { text-align: center; padding: 2rem; color: var(--mat-sys-on-surface-variant, #666); }
  `],
})
export class HolidayListComponent implements OnInit {
  private holidayService = inject(HolidayService);

  holidays = signal<HolidayDto[]>([]);
  showForm = signal(false);
  newName = '';
  newDate = '';
  newRecurring = false;
  selectedYear = new Date().getFullYear();
  years = Array.from({ length: 10 }, (_, i) => new Date().getFullYear() + i - 2);
  columns = ['name', 'date', 'recurring', 'actions'];

  ngOnInit(): void {
    this.loadHolidays();
  }

  loadHolidays(): void {
    this.holidayService.getList(this.selectedYear || undefined).subscribe(h => this.holidays.set(h));
  }

  addHoliday(): void {
    this.holidayService.create({
      name: this.newName,
      date: this.newDate,
      isRecurringYearly: this.newRecurring,
    }).subscribe({
      next: () => {
        this.cancelAdd();
        this.loadHolidays();
      },
    });
  }

  deleteHoliday(id: string): void {
    if (confirm('Delete this holiday?')) {
      this.holidayService.delete(id).subscribe(() => this.loadHolidays());
    }
  }

  cancelAdd(): void {
    this.showForm.set(false);
    this.newName = '';
    this.newDate = '';
    this.newRecurring = false;
  }
}
