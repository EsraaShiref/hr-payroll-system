import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LeaveRequestDto, LeaveBalanceDto } from '../../models/attendance';

@Injectable({ providedIn: 'root' })
export class LeaveRequestService {
  private readonly base = `${environment.apiBaseUrl}/leave-requests`;

  constructor(private http: HttpClient) {}

  getPending(departmentId?: string): Observable<LeaveRequestDto[]> {
    let params = new HttpParams();
    if (departmentId) params = params.set('departmentId', departmentId);
    return this.http.get<LeaveRequestDto[]>(`${this.base}/pending`, { params });
  }

  getBalances(employeeId: string): Observable<LeaveBalanceDto[]> {
    return this.http.get<LeaveBalanceDto[]>(`${this.base}/balances/${employeeId}`);
  }

  submit(request: SubmitLeaveRequest): Observable<string> {
    return this.http.post<string>(this.base, request);
  }

  approve(id: string, approvedBy: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/approve`, { approvedBy });
  }

  reject(id: string, rejectedBy: string, reason: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/reject`, { rejectedBy, reason });
  }

  cancel(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/cancel`, {});
  }
}

export interface SubmitLeaveRequest {
  employeeId: string;
  leaveType: string;
  startDate: string;
  endDate: string;
  reason: string | null;
}
