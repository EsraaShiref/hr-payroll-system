import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ContractDto, ContractVersionDto } from '../../models/hr';

@Injectable({ providedIn: 'root' })
export class ContractService {
  private readonly base = `${environment.apiBaseUrl}/contracts`;

  constructor(private http: HttpClient) {}

  getById(id: string): Observable<ContractDto> {
    return this.http.get<ContractDto>(`${this.base}/${id}`);
  }

  getActiveForEmployee(employeeId: string): Observable<ContractDto> {
    return this.http.get<ContractDto>(`${this.base}/active/${employeeId}`);
  }

  getVersionForDate(employeeId: string, effectiveDate: string): Observable<ContractVersionDto> {
    return this.http.get<ContractVersionDto>(
      `${this.base}/version/${employeeId}`,
      { params: { effectiveDate } },
    );
  }

  getVersions(contractId: string): Observable<ContractVersionDto[]> {
    return this.http.get<ContractVersionDto[]>(`${this.base}/${contractId}/versions`);
  }
}
