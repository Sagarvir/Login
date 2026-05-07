import { Component } from '@angular/core';
import { AuthService } from '../core/services/auth.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import Swal from 'sweetalert2';
@Component({
  selector: 'app-assign-role',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './assign-role.html',
  styleUrls: ['./assign-role.css']
})
export class AssignRoleComponent {

  employeeId = '';
  roleName = '';
  availableRoles = ['Admin', 'Translator', 'Creator', 'Viewer'];

  constructor(private authService: AuthService) {}

  assignRole() {
    if (!this.employeeId.trim() || !this.roleName) {
      Swal.fire({
        icon: 'warning',
        title: 'Incomplete details',
        text: 'Please fill in employee ID and select a role.',
      });
      return;
    }

    this.authService.assignRole({
      employeeId: this.employeeId,
      roleName: this.roleName
    }).subscribe({
      next: () => {
        Swal.fire({
          icon: 'success',
          title: 'Role assigned',
          text: `Role '${this.roleName}' assigned to ${this.employeeId}.`,
          timer: 1800,
          showConfirmButton: false,
        });
      },
      error: (err) => {
        const message = err.error?.message || err.message || 'Unable to assign role.';
        Swal.fire({
          icon: 'error',
          title: 'Assignment failed',
          text: message,
        });
      }
    });
  }
}