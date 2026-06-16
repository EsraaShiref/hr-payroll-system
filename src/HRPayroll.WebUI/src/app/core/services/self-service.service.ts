import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  MyPayslipDto, MyLeaveRequestDto, SubmitMyLeaveRequest,
  DisputeAttendanceRequest, UpdateMyProfileRequest, RequestEmailChangeRequest,
} from '../../models/self-service';
import { AttendanceViewerResult, LeaveBalanceDto } from '../../models/attendance';

@Injectable({ providedIn: 'root' })
export class SelfServiceService {
  private readonly base = `${environment.apiBaseUrl}/my`;
  constructor(private http: HttpClient) {}

  getMyPayslips(): Observable<MyPayslipDto[]> {
    return this.http.get<MyPayslipDto[]>(`${this.base}/payslips`);
  }

  downloadPayslipPdf(runId: string): Observable<Blob> {
    return this.http.get(`${this.base}/payslips/${runId}/pdf`, { responseType: 'blob' });
  }

  submitLeave(request: SubmitMyLeaveRequest): Observable<string> {
    return this.http.post<string>(`${this.base}/leave`, request);
  }

  getMyLeaveRequests(year?: number, status?: string): Observable<MyLeaveRequestDto[]> {
    let params = new HttpParams();
    if (year) params = params.set('year', year);
    if (status) params = params.set('status', status);
    return this.http.get<MyLeaveRequestDto[]>(`${this.base}/leave`, { params });
  }

  getMyLeaveBalances(): Observable<LeaveBalanceDto[]> {
    return this.http.get<LeaveBalanceDto[]>(`${this.base}/leave/balances`);
  }

  getMyAttendance(year: number, month: number): Observable<AttendanceViewerResult> {
    const params = new HttpParams().set('year', year).set('month', month);
    return this.http.get<AttendanceViewerResult>(`${this.base}/attendance`, { params });
  }

  disputeAttendance(request: DisputeAttendanceRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/attendance/dispute`, request);
  }

  updateProfile(request: UpdateMyProfileRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/profile`, request);
  }

  requestEmailChange(request: RequestEmailChangeRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/profile/email`, request);
  }
}
