import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatPaginator } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatCardModule } from '@angular/material/card';
import { TranslationService } from '../../services/translation.service';
import { Translation } from '../../models/translation.model';
import { DeleteConfirmDialogComponent } from '../delete-confirm-dialog/delete-confirm-dialog.component';
import { AddTranslationDialogComponent } from '../add-translation-dialog/add-translation-dialog.component';

@Component({
  selector: 'app-translation-table',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatTooltipModule,
    MatSnackBarModule,
    MatPaginatorModule,
    MatSortModule,
    MatDialogModule,
    MatCardModule,
  ],
  templateUrl: './translation-table.component.html',
  styleUrl: './translation-table.component.scss',
})
export class TranslationTableComponent implements OnInit, AfterViewInit {
  displayedColumns: string[] = [
    'translationKey',
    'originalText',
    'translation',
    'tags',
    'actions',
  ];
  dataSource = new MatTableDataSource<Translation>();
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  searchTerm = '';

  constructor(
    private translationService: TranslationService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.translationService.getTranslations().subscribe((translations) => {
      this.dataSource.data = translations;
      if (this.paginator) {
        this.dataSource.paginator = this.paginator;
      }
    });
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.searchTerm = filterValue.trim().toLowerCase();
    this.dataSource.filter = this.searchTerm;
  }

  updateTranslation(index: number, translation: Translation): void {
    this.translationService.updateTranslation(index, translation);
  }

  deleteTranslation(index: number): void {
    const dialogRef = this.dialog.open(DeleteConfirmDialogComponent);

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.translationService.deleteTranslation(index);
        this.snackBar.open('Translation deleted successfully', 'Close', {
          duration: 2000,
        });
      }
    });
  }

  addNewTranslation(): void {
    const dialogRef = this.dialog.open(AddTranslationDialogComponent, {
      width: '520px',
    });

    dialogRef.afterClosed().subscribe((result: Translation | undefined) => {
      if (!result) {
        return;
      }

      this.translationService.addTranslation({
        translationKey: result.translationKey || '',
        originalText: result.originalText || '',
        translation: '',
        tags: result.tags || '',
      });

      this.snackBar.open('New translation added', 'Close', {
        duration: 2000,
      });
    });
  }
}
