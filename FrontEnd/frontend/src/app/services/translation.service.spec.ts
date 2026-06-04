import { TestBed } from '@angular/core/testing';
import { TranslationService } from './translation.service';
import { HttpClient } from '@angular/common/http';
import { of, throwError, firstValueFrom } from 'rxjs';
import { vi } from 'vitest';

describe('TranslationService', () => {
  let service: TranslationService;
  let httpMock: any;

  beforeEach(() => {
    httpMock = {
      get: vi.fn(),
      post: vi.fn(),
      put: vi.fn(),
      delete: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        TranslationService,
        { provide: HttpClient, useValue: httpMock },
      ],
    });

    service = TestBed.inject(TranslationService);
  });

  it('should create', () => {
    expect(service).toBeTruthy();
  });

  // ✅ Language
  it('should set and get selected language', () => {
    service.setSelectedLanguage('FR');
    expect(service.getSelectedLanguage()).toBe('FR');
  });

  // ✅ Save coordination
  it('should emit saveRequested$', async () => {
    const promise = firstValueFrom(service.saveRequested$);
    service.requestSave();
    await promise;
    expect(true).toBeTruthy();
  });

  it('should emit saveCompleted$', async () => {
    const promise = firstValueFrom(service.saveCompleted$);
    service.notifySaveCompleted();
    await promise;
    expect(true).toBeTruthy();
  });

  // ✅ loadTranslations success
  it('should load translations and update state', async () => {
    httpMock.get.mockReturnValue(of([
      { keyName: 'HELLO', originalText: 'Hello', translation: 'Bonjour' }
    ]));

    const res = await firstValueFrom(service.loadTranslations());

    expect(res.length).toBe(1);
    expect(res[0].translationKey).toBe('HELLO');
  });

  // ✅ loadTranslations error
  it('should handle loadTranslations error', async () => {
    httpMock.get.mockReturnValue(throwError(() => new Error('error')));

    const res = await firstValueFrom(service.loadTranslations());

    expect(res).toEqual([]);
  });

  // ✅ getAllTranslations
  it('should map key-value dictionary', async () => {
    httpMock.get.mockReturnValue(of([
      { key: 'HELLO', value: 'Bonjour' }
    ]));

    const res = await firstValueFrom(service.getAllTranslations('EN'));

    expect(res['HELLO']).toBe('Bonjour');
  });

  // ✅ addTranslation
  it('should call POST on addTranslation', async () => {
    httpMock.post.mockReturnValue(of({}));
    const spy = vi.spyOn(service, 'loadTranslations').mockReturnValue(of([]));

    await firstValueFrom(service.addTranslation({
      translationKey: 'HELLO',
      originalText: 'Hello',
      translation: '',
      isModified: false,
      projectId: 1,
      tags: '1',
      client: '',
      project: '',
    } as any));

    expect(httpMock.post).toHaveBeenCalled();
    expect(httpMock.post).toHaveBeenCalledWith(
      'https://localhost:7199/api/TranslationKey',
      {
        keyName: 'HELLO',
        originalText: 'Hello',
        projectId: 1,
      }
    );
    expect(spy).toHaveBeenCalled();
  });

  // ✅ updateTranslation (API)
  it('should update translation via API when id exists', async () => {
    httpMock.put.mockReturnValue(of({
      id: 1,
      keyName: 'HELLO',
      originalText: 'Hello',
      translation: 'Hi'
    }));

    service['translations'] = [{
      id: 1,
      translationKey: 'HELLO',
      originalText: 'Hello',
      translation: '',
      isModified: false,
      projectId: 1,
      tags: '1',
      client: '',
      project: '',
    }];

    await firstValueFrom(
      service.updateTranslation(0, service['translations'][0])
    );

    expect(httpMock.put).toHaveBeenCalled();
  });

  // ✅ deleteTranslation (API)
  it('should delete translation via API', async () => {
    httpMock.delete.mockReturnValue(of(undefined));

    service['translations'] = [{
      id: 1,
      translationKey: 'HELLO',
      originalText: 'Hello',
      translation: '',
      isModified: false,
      projectId: 1,
      tags: '1',
      client: '',
      project: '',
    }];

    await firstValueFrom(service.deleteTranslation(0));

    expect(httpMock.delete).toHaveBeenCalled();
  });

  // ✅ stats
  it('should calculate stats correctly', () => {
    service['translations'] = [
      { translationKey: 'A', translation: 'a' },
      { translationKey: 'B', translation: '' },
    ] as any;

    const stats = service.getStats();

    expect(stats.totalKeys).toBe(2);
    expect(stats.translated).toBe(1);
    expect(stats.completion).toBe(50);
  });

  // ✅ upsertTranslations finalize
  it('should call notifySaveCompleted after upsert', async () => {
    httpMock.post.mockReturnValue(of('ok'));
    const spy = vi.spyOn(service, 'notifySaveCompleted');

    await firstValueFrom(service.upsertTranslations([]));

    expect(spy).toHaveBeenCalled();
  });

});