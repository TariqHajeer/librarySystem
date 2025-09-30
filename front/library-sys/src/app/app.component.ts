import { Component, Inject, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { FloatLabelModule } from 'primeng/floatlabel';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { BookService } from '../services/book.service';
import { UserService } from '../services/user.service';
import { Book } from '../interfaces/Book';
import { map, Observable, tap } from 'rxjs';
import { UserListDto } from '../interfaces/UserDto';
import { AsyncPipe } from '@angular/common';
import { environment } from '../environments/environment.development';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    FormsModule,
    TableModule,
    FloatLabelModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    SelectModule,
    AsyncPipe,
    ToastModule
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  providers: [
    MessageService
  ]
})
export class AppComponent implements OnInit {

  books: any[] = [
    {
      id: 1,
      title: 'The Catcher in the Rye',
      author: 'J.D. Salinger',
      year: 1951,
      genre: 'Fiction',
      totalCopies: 898,
      availableCopies: 7868,
    },
    {
      id: 2,
      title: 'Brave New World',
      author: 'Aldous Huxley',
      year: 1932,
      genre: 'Science Fiction',
      totalCopies: 53225,
      availableCopies: 42423,
    },
    {
      id: 3,
      title: 'The Lord of the Rings',
      author: 'J.R.R. Tolkien',
      year: 1954,
      genre: 'Fantasy',
      totalCopies: 42242,
      availableCopies: 22444,
    },
    {
      id: 4,
      title: 'The Da Vinci Code',
      author: 'Dan Brown',
      year: 2003,
      genre: 'Mystery',
      totalCopies: 97878,
      availableCopies: 8000,
    },
    {
      id: 5,
      title: 'The Alchemist',
      author: 'Paulo Coelho',
      year: 1988,
      genre: 'Fiction',
      totalCopies: 20000,
      availableCopies: 6000,
    },
  ];
  title: string = "";
  author: string = "";
  isbn: string = "";
  selectedBook!: Book

  borrowDialog: boolean = false;
  retrieveDialog: boolean = false;
  user: any;

  ngOnInit(): void {

    this.users$ = this.userService.getUsers();
  }
  /**
   *
   */
  constructor(private messageService: MessageService) {

  }
  private bookService = inject(BookService);

  private userService = inject(UserService);
  books$!: Observable<Book[]>;
  users$!: Observable<UserListDto[]>
  totalRec$!: Observable<number>;
  lazyLoad(event?: TableLazyLoadEvent) {
    let page = event?.first ? event.first : 0;
    let pageSize = event?.rows ? event.rows : 5;
    page = page / pageSize;
    console.log(event);
    this.getBooks(page, pageSize);
  }
  getBooks(page: number, pageSize: number) {
    page++;
    this.books$ = this.bookService.getBooks(this.title, this.author, this.isbn, page, pageSize).pipe(map(c => c.data));
    this.totalRec$ = this.bookService.getBooks(this.title, this.author, this.isbn, page, pageSize).pipe(map(c => c.totalCount));
  }
  showBorrowDialog(book: Book) {
    this.borrowDialog = true;
    this.selectedBook = book;
  }
  showRetrieveDialog(book: Book) {
    this.retrieveDialog = true;
    this.selectedBook = book;
  }
  onBorrowBook() {

    this.bookService.borrowBook(this.user.id, this.selectedBook)
      .subscribe({
        next: (res) => {
          this.messageService.add({ severity: 'success', summary: 'Success Message', detail: 'done', key: 'tl', life: 3000 });
        }, error: (err) => {
          
          this.messageService.add({ severity: 'error', summary: 'Success Message', detail: err.error.error, key: 'tl', life: 3000 });
        }
      });
  }
  onRetrieveBook() {
    this.bookService.retrieveBook(this.user, this.selectedBook).subscribe({
      next: (res) => {
        this.messageService.add({ severity: 'success', summary: 'Success Message', detail: 'done', key: 'tl', life: 3000 });
      }, error: (err) => {

        this.messageService.add({ severity: 'error', summary: 'Success Message', detail: err.error.error, key: 'tl', life: 3000 });
      }
    });
  }
  refreshTable(): void {
    this.lazyLoad();
  }
}
