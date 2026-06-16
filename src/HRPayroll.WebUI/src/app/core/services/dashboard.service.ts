import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  DashboardAttendanceSummary, PendingLeaveRequest, PayrollBudgetSummary,
  HeadcountTrend, UpcomingContractRenewal,
} from '../../models/dashboard';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly base = `${environment.apiBaseUrl}/dashboard`;

  constructor(private http: HttpClient) {}

  getAttendanceSummary(date?: string): Observable<DashboardAttendanceSummary> {
    let params = new HttpParams();
    if (date) params = params.set('date', date);
    return this.http.get<DashboardAttendanceSummary>(`${this.base}/attendance-summary`, { params });
  }

  getPendingLeave(): Observable<PendingLeaveRequest[]> {
    return this.http.get<PendingLeaveRequest[]>(`${this.base}/pending-leave`);
  }

  getPayrollBudget(year: number, month: number): Observable<PayrollBudgetSummary> {
    return this.http.get<PayrollBudgetSummary>(
      `${this.base}/payroll-budget`,
      { params: new HttpParams().set('year', year).set('month', month) },
    );
  }

  getHeadcountTrend(): Observable<HeadcountTrend> {
    return this.http.get<HeadcountTrend>(`${this.base}/headcount-trend`);
  }

  getContractRenewals(): Observable<UpcomingContractRenewal[]> {
    return this.http.get<UpcomingContractRenewal[]>(`${this.base}/contract-renewals`);
  }
}
