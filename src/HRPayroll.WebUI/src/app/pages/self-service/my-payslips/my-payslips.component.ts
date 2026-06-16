import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { SelfServiceService } from '../../../core/services/self-service.service';
import { MyPayslipDto } from '../../../models/self-service';

@Component({
  selector: 'app-my-payslips',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatCardModule, MatTableModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>My Payslips</h1>
      </div>

      <mat-card>
        <mat-card-content>
          @if (loading()) {
            <div class="loading"><mat-spinner diameter="32" /></div>
          } @else {
            <table mat-table [dataSource]="payslips()" class="full-width">
              <ng-container matColumnDef="period">
                <th mat-header-cell *matHeaderCellDef>Period</th>
                <td mat-cell *matCellDef="let p">{{ p.period }}</td>
              </ng-container>

              <ng-container matColumnDef="baseSalary">
                <th mat-header-cell *matHeaderCellDef>Base Salary</th>
                <td mat-cell *matCellDef="let p">{{ p.baseSalary | number:'1.2-2' }}</td>
              </ng-container>

              <ng-container matColumnDef="grossPay">
                <th mat-header-cell *matHeaderCellDef>Gross Pay</th>
                <td mat-cell *matCellDef="let p">{{ p.grossPay | number:'1.2-2' }}</td>
              </ng-container>

              <ng-container matColumnDef="totalDeductions">
                <th mat-header-cell *matHeaderCellDef>Deductions</th>
                <td mat-cell *matCellDef="let p">{{ p.totalDeductions | number:'1.2-2' }}</td>
              </ng-container>

              <ng-container matColumnDef="netPay">
                <th mat-header-cell *matHeaderCellDef>Net Pay</th>
                <td mat-cell *matCellDef="let p"><strong>{{ p.netPay | number:'1.2-2' }}</strong></td>
              </ng-container>

              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef></th>
                <td mat-cell *matCellDef="let p">
                  <button mat-icon-button (click)="download(p.runId)" matTooltip="Download PDF">
                    <mat-icon>download</mat-icon>
                  </button>
                </td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="columns"></tr>
              <tr mat-row *matRowDef="let row; columns: columns;"></tr>
            </table>

            @if (payslips().length === 0) {
              <div class="empty">No finalized payslips available.</div>
            }
          }
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .page-container { padding: 1.5rem; }
    .page-header { margin-bottom: 1rem; }
    .page-header h1 { margin: 0; font-size: 1.5rem; font-weight: 400; }
    .full-width { width: 100%; }
    .loading { display: flex; justify-content: center; padding: 2rem; }
    .empty { text-align: center; padding: 2rem; color: var(--mat-sys-on-surface-variant, #666); }
  `],
})
export class MyPayslipsComponent implements OnInit {
  private service = inject(SelfServiceService);
  payslips = signal<MyPayslipDto[]>([]);
  loading = signal(true);
  columns = ['period', 'baseSalary', 'grossPay', 'totalDeductions', 'netPay', 'actions'];

  ngOnInit(): void {
    this.service.getMyPayslips().subscribe({
      next: p => { this.payslips.set(p); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  download(runId: string): void {
    this.service.downloadPayslipPdf(runId).subscribe(blob => {
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `payslip-${runId}.pdf`;
      a.click();
      URL.revokeObjectURL(url);
    });
  }
}
