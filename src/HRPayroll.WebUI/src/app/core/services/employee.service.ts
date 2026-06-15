import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  PaginatedList,
  EmployeeDto,
  EmployeeDetailDto,
} from '../../models/hr';

@Injectable({ providedIn: 'root' })
export class EmployeeService {
  private readonly base = `${environment.apiBaseUrl}/employees`;

  constructor(private http: HttpClient) {}

  getList(params: {
    pageIndex: number;
    pageSize: number;
    sortField?: string;
    sortDirection?: string;
    searchTerm?: string;
    departmentId?: string;
    employmentStatus?: string;
  }): Observable<PaginatedList<EmployeeDto>> {
    let p = new HttpParams()
      .set('pageIndex', params.pageIndex)
      .set('pageSize', params.pageSize);

    if (params.sortField) p = p.set('sortField', params.sortField);
    if (params.sortDirection) p = p.set('sortDirection', params.sortDirection);
    if (params.searchTerm) p = p.set('searchTerm', params.searchTerm);
    if (params.departmentId) p = p.set('departmentId', params.departmentId);
    if (params.employmentStatus) p = p.set('employmentStatus', params.employmentStatus);

    return this.http.get<PaginatedList<EmployeeDto>>(this.base, { params: p });
  }

  getById(id: string): Observable<EmployeeDetailDto> {
    return this.http.get<EmployeeDetailDto>(`${this.base}/${id}`);
  }
}
