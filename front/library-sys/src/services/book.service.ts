import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Book } from '../interfaces/Book';
import { PageList } from '../interfaces/pagedList';
import { environment } from '../environments/environment.development';

@Injectable({
  providedIn: 'root',
})
export class BookService {
  constructor(private http: HttpClient) { }

  getBooks(
    title: string,
    author: string,
    isbn: string,
    startRow: number,
    pageSize: number
  ): Observable<PageList<Book>> {
    const params = new HttpParams().set('title', title)
      .append('author', author)
      .append('isbn', isbn)
      .append('page', startRow)
      .append('pageSize', pageSize)
    return this.http.get<PageList<Book>>(`${environment.apiUrl}book`, { params });
  }

  borrowBook(userId: string, book: Book): Observable<Book> {
    const nowUtc = new Date().toISOString();
    let data = {
      bookId: book.id,
      userId: userId,
      borrowDate:nowUtc,
    };
    return this.http.post<Book>(`${environment.apiUrl}book/Borrow`, data);
  }

  retrieveBook(userId: string, book: Book): Observable<Book> {
    const nowUtc = new Date().toISOString();
    let data = {
      bookId: book.id,
      userId: userId,
      returnDate:nowUtc,
    };
    return this.http.post<Book>(`${environment.apiUrl}book/ReturnBook`, data);
  }
}
