﻿import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ResourceDataService } from '../dataservices/resource-data.service';
import { UserPermissionService } from '../dataservices/userpermission-data.service';
import { IChildElement } from '../interfaces/htmlInterfaces';
import { IResourceInfo } from '../interfaces/resourceInfo';

@Component({
    template: `
        <div class="row">
            <h3>Resources Archive</h3>
        </div>
        <div class="row">
            <div *ngFor="let resource of resources" class="col-sm-6 col-md-4" >
                <h5>{{resource.name}}</h5>
                <p>Created:{{resource.timeStamp | date:"MM/dd/yy" }}</p>
                <button type="button" class="btn btn-primary" *ngIf="canDownloadArchive" (click)="downloadResource(resource.name)"><span class="glyphicon-btn glyphicon glyphicon-download-alt"></span></button>
                <button type="button" class="btn btn-primary" *ngIf="canDeleteArchives" (click)="delete(resource.name)"><span class="glyphicon-btn glyphicon glyphicon-trash"></span></button>
            </div>
        </div>
        <hr />
        <div class="row" *ngIf="canDeleteArchives">
            <div class="input-group col-sm-4 col-md-3">
                <span class="input-group-btn">
                    <button type="button" class="btn btn-primary" (click)="deleteBefore()">Delete before</button>
                </span>
                <input class="form-control"  type="date" #input value="{{inputDate | date:'yyyy-MM-dd'}}"  (blur)="onBlur()">
            </div>
        </div>
        <hr />
        <div class="row">
            <button type="button" class="btn btn-primary" (click)="back()">Back</button>
        </div>
`,
})
export class ResourceArchiveComponent implements OnInit {
    public resources: IResourceInfo[];
    public clientId: string;
    public inputDate: Date;
    @ViewChild('input')
    public input: IChildElement<HTMLInputElement>;
    public canDeleteArchives = false;
    public canDownloadArchive = false;
    constructor(private dataService: ResourceDataService, private permissionService: UserPermissionService, private route: ActivatedRoute, private router: Router) {
        this.inputDate = new Date();
    }

    public ngOnInit() {
        this.route.params.forEach((value) => {
            this.clientId = value['clientId'];
            this.updateArchiveList();
        });
        this.permissionService.getPermissionForClient(this.clientId)
            .then((permissions) => {
                this.canDeleteArchives = permissions.canDeleteArchives;
                this.canDownloadArchive = permissions.hasClientConfiguratorClaim;
            });
    }

    public downloadResource(file: string) {
        window.open('ResourceArchive/' + this.clientId + '/' + file);
    }

    public delete(file: string) {
        const result = confirm('Are you sure you want to delete ' + file + '?');
        if (!result) {
            return;
        }
        this.dataService.deleteArchivedResource(this.clientId, file).then(() => this.updateArchiveList());
    }

    public deleteBefore() {
        this.dataService.deleteArchivedResourceBefore(this.clientId, this.inputDate).then(() => this.updateArchiveList());
    }

    public back() {
        this.router.navigate(['/client/' + this.clientId]);
    }

    public onBlur() {
        this.inputDate = new Date(this.input.nativeElement.value);
    }

    private updateArchiveList() {
        this.dataService.getClientArchiveResourceInfo(this.clientId)
            .then((returnedResources) => this.resources = returnedResources);
    }
}
