import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { TranslationService } from '../../services/translation.service';
import { DashboardStats } from '../../models/translation.model';
import { HeaderComponent } from '../header/header.component';
import { TranslationTableComponent } from '../translation-table/translation-table.component';
import { FooterComponent } from '../footer/footer.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatProgressBarModule,
    HeaderComponent,
    TranslationTableComponent,
    FooterComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  stats: DashboardStats = {
    totalKeys: 0,
    translated: 0,
    completion: 0,
  };

  constructor(private translationService: TranslationService) {}

  ngOnInit(): void {
    this.translationService.loadTranslations().subscribe({
      next: () => this.updateStats(),
      error: () => this.updateStats()
    });

    this.translationService.translations$.subscribe(() => {
      this.updateStats();
    });
  }

  updateStats(): void {
    this.stats = this.translationService.getStats();
  }
}
