import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { UserListDto } from '../interfaces/UserDto';
import { map, Observable } from 'rxjs';
import { environment } from '../environments/environment.development';
import { PageList } from '../interfaces/pagedList';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  constructor(private http: HttpClient) { }

  getUsers(): Observable<UserListDto[]> {
    return this.http.get<PageList<UserListDto>>(`${environment.apiUrl}User`).pipe(map(c=>c.data))
  }
}
