import { AdminService } from './../../_services/admin.service';
import { Component, OnInit } from '@angular/core';
import { Photo } from 'src/app/_models/photo';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css'],
})
export class PhotoManagementComponent implements OnInit {
  photos: Photo[]; // a list of unapproved photos
  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.getPhotosForApproval();
  }
  getPhotosForApproval() {
    this.adminService.getPhotosForApproval().subscribe((photos) => {
      console.log('resp from getPhotos', photos);
      this.photos = photos;
    });
  }
  approvePhoto(photoId: number) {
    this.adminService.approvePhoto(photoId).subscribe(() => {
      this.photos.splice(
        this.photos.findIndex((x) => x.id === photoId),
        1
      );
    });
  }
  rejectPhoto(photoId: number) {
    this.adminService.rejectPhoto(photoId).subscribe(() => {
      this.photos.splice(
        this.photos.findIndex((x) => x.id === photoId),
        1
      );
    });
  }
}
