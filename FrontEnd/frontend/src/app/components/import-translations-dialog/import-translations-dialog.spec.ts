import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ImportTranslationsDialog } from './import-translations-dialog';

describe('ImportTranslationsDialog', () => {
  let component: ImportTranslationsDialog;
  let fixture: ComponentFixture<ImportTranslationsDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ImportTranslationsDialog],
    }).compileComponents();

    fixture = TestBed.createComponent(ImportTranslationsDialog);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
