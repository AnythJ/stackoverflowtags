import { Component, OnInit, ViewChild } from '@angular/core';
import { TagsService, Tag } from '../../services/tags.service';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';

@Component({
  selector: 'app-tags-table',
  templateUrl: './tags-table.component.html',
  styleUrls: ['./tags-table.component.css']
})
export class TagsTableComponent implements OnInit {
  displayedColumns: string[] = ['name', 'count', 'share'];
  dataSource = new MatTableDataSource<Tag>([]);
  totalItems = 0;
  pageSize = 20;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(private tagsService: TagsService) {}

  ngOnInit() {
    this.refresh();
    this.loadTags();
  }

  ngAfterViewInit() {
    this.paginator.page.subscribe(() => this.loadTags());
    this.sort.sortChange.subscribe(() => {
      this.paginator.pageIndex = 0;
      this.loadTags();
    });
  }

  loadTags() {
    const page = this.paginator ? this.paginator.pageIndex + 1 : 1;
    const pageSize = this.paginator ? this.paginator.pageSize : 20;
    const sortBy = this.sort ? this.sort.active : 'share';
    const sortOrder = this.sort ? this.sort.direction || 'desc' : 'desc';

    this.tagsService.getTags(page, pageSize, sortBy, sortOrder)
      .subscribe(res => {
        console.log(res)
        this.dataSource.data = res.items;
        this.totalItems = res.totalCount;
      });
  }

  refresh() {
    this.tagsService.refreshTags().subscribe(() => {
      this.paginator.pageIndex = 0;
      this.loadTags();
    });
  }
}
