import { Component, OnInit } from "@angular/core";
import { MatDialog } from "@angular/material/dialog";
import { ActivatedRoute, Router } from "@angular/router";
import { AppState } from "@app/app-state/models/app-state";
import { SepApplication } from "@models/sep-application.model";
import { User } from "@models/user.model";
import { Store } from "@ngrx/store";
import { IndexedDBService } from "@services/indexed-db.service";
import { StarterChecklistComponent } from "../starter-checklist/starter-checklist.component";
import { SepApplicationSummary } from "@models/sep-application-summary.model";

@Component({
  selector: "app-my-applications",
  templateUrl: "./my-applications.component.html",
  styleUrls: ["./my-applications.component.scss"],
})
export class MyApplicationsComponent implements OnInit {
  displayedColumns = ["status", "info", "actions"];
  currentUser: User;
  submittedApplication: SepApplicationSummary[];

  constructor(
    private store: Store<AppState>,
    private db: IndexedDBService,
    private route: ActivatedRoute,
    private router: Router,
    private dialog: MatDialog
  ) {
    store
      .select((state) => state.currentUserState.currentUser)
      .subscribe((user: User) => {
        this.currentUser = user;
      });
  }

  async ngOnInit() {}

  startApplication() {
    const dialogConfig = {
      disableClose: true,
      autoFocus: true,
      width: "600px",
      data: {
        showStartApp: true,
      },
    };

    // open dialog, get reference and process returned data from dialog
    const dialogRef = this.dialog.open(StarterChecklistComponent, dialogConfig);

    dialogRef.afterClosed().subscribe((startApplication: boolean) => {
      if (!startApplication) {
        return;
      }

      const data = {
        dateCreated: new Date(),
      } as SepApplication;

      this.db.saveSepApplication(data).then((localId) => {
        this.router.navigateByUrl(`/sep/application/${localId}/applicant`);
      });
    });
  }
}
