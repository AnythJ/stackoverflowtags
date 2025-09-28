import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Tag {
  name: string;
  count: number;
  share: number;
  fetchedAt: string;
}

export interface TagResponse {
  items: Tag[];
  totalCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class TagsService {
  private apiUrl = 'http://localhost:8080/api/tags'; 

  constructor(private http: HttpClient) {}

  getTags(page = 1, pageSize = 20, sortBy = 'share', sortOrder = 'desc'): Observable<TagResponse> {
    return this.http.get<TagResponse>(`${this.apiUrl}?page=${page}&pageSize=${pageSize}&sortBy=${sortBy}&sortOrder=${sortOrder}`);
  }

  refreshTags(): Observable<any> {
    return this.http.post(`${this.apiUrl}/refresh`, {});
  }
}
