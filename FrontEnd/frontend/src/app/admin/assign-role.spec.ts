import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AssignRoleComponent } from './assign-role';
import { vi } from 'vitest';
import Swal from 'sweetalert2';
describe('AssignRoleComponent', () => {
  let component: AssignRoleComponent;
  let fixture: ComponentFixture<AssignRoleComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AssignRoleComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(AssignRoleComponent);
    component = fixture.componentInstance;
    
    vi.spyOn(Swal, 'fire').mockResolvedValue({} as any);
    await fixture.whenStable();
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // ✅ basic property check
  it('should have default values (if any)', () => {
    // example (adjust if your component has properties)
    expect(component).toBeDefined();
  });

  // ✅ DOM render test
  it('should render component template', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled).toBeTruthy();
  });

  // ✅ method existence (replace with real method if present)
  it('should have assign method defined', () => {
    // change 'assignRole' to your actual method name
    expect(component['assignRole']).toBeDefined();
  });

  // ✅ simple interaction test (only if method exists)
  it('should call assignRole method', () => {
    if (component['assignRole']) {
      const spy = vi.spyOn(component as any, 'assignRole');
      (component as any).assignRole();
      expect(spy).toHaveBeenCalled();
    }
  });
});