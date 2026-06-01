import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AddLanguageComponent } from './add-language';
import { HttpClient } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import Swal from 'sweetalert2';
import { Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';

describe('AddLanguageComponent', () => {
  let component: AddLanguageComponent;
  let fixture: ComponentFixture<AddLanguageComponent>;
  let httpSpy: any;
  let router: Router;

  // ✅ Fix SweetAlert issue
  beforeAll(() => {
    (window as any).matchMedia = () => ({
      matches: false,
      addListener: () => {},
      removeListener: () => {}
    });
  });

  beforeEach(async () => {
    httpSpy = {
      post: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [AddLanguageComponent, RouterTestingModule],
      providers: [{ provide: HttpClient, useValue: httpSpy }]
    }).compileComponents();

    fixture = TestBed.createComponent(AddLanguageComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);

    // ✅ mock SweetAlert
    vi.spyOn(Swal, 'fire').mockResolvedValue({} as any);

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have empty initial values', () => {
    expect(component.languageCode).toBe('');
    expect(component.languageName).toBe('');
    expect(component.isLoading).toBeFalsy();
  });

  it('should show warning if fields are empty', () => {
    component.languageCode = '';
    component.languageName = '';

    component.addLanguage();

    expect(Swal.fire).toHaveBeenCalled();
    expect(httpSpy.post).not.toHaveBeenCalled();
  });

  it('should call http.post when valid data is provided', () => {
    component.languageCode = 'EN';
    component.languageName = 'English';

    httpSpy.post.mockReturnValue(of({}));

    component.addLanguage();

    expect(httpSpy.post).toHaveBeenCalledWith(
      'https://localhost:7199/api/Language',
      {
        id: 0,
        name: 'English'
      }
    );
  });

  it('should reset fields on success', () => {
    component.languageCode = 'EN';
    component.languageName = 'English';

    httpSpy.post.mockReturnValue(of({}));

    component.addLanguage();

    expect(component.languageCode).toBe('');
    expect(component.languageName).toBe('');
    expect(component.isLoading).toBeFalsy();
  });

  it('should handle error properly', () => {
    component.languageCode = 'EN';
    component.languageName = 'English';

    const mockError = {
      error: { message: 'Already exists' }
    };

    httpSpy.post.mockReturnValue(throwError(() => mockError));

    component.addLanguage();

    expect(component.isLoading).toBeFalsy();
    expect(Swal.fire).toHaveBeenCalled();
  });
});