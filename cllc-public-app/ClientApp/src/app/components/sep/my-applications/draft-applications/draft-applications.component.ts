import { Component, OnInit, ViewChild } from "@angular/core";
import { MatDialog } from "@angular/material/dialog";
import { MatPaginator } from "@angular/material/paginator";
import { MatSort } from "@angular/material/sort";
import { MatTableDataSource } from "@angular/material/table";
import { Router } from "@angular/router";
import { CancelSepApplicationDialogComponent } from "@components/sep/sep-application/cancel-sep-application-dialog/cancel-sep-application-dialog.component";
import { SEP_APPLICATION_STEPS } from "@components/sep/sep-application/sep-application.component";
import { faCopy, faPencilAlt } from "@fortawesome/free-solid-svg-icons";
import {
  SepDraftApplicationSummary,
  SepDraftApplicationTableElement,
} from "@models/sep-application-summary.model";
import { SepApplication } from "@models/sep-application.model";
import { IndexedDBService } from "@services/indexed-db.service";
import { SpecialEventsDataService } from "@services/special-events-data.service";
import { isBefore } from "date-fns";

/**
 * Component for displaying draft special event applications in a table format.
 *
 * @export
 * @class DraftApplicationsComponent
 * @implements {OnInit}
 */
@Component({
  selector: "app-draft-applications",
  templateUrl: "./draft-applications.component.html",
  styleUrls: ["./draft-applications.component.scss"],
})
export class DraftApplicationsComponent implements OnInit {
  faCopy = faCopy;
  faPencilAlt = faPencilAlt;

  @ViewChild(MatSort, { static: true }) sort: MatSort;
  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;

  dataSource = new MatTableDataSource<SepDraftApplicationTableElement>();

  columnsToDisplay = [
    "eventStatusLabel",
    "eventName",
    "eventStartDate",
    "dateSubmitted",
    "actions",
  ];

  constructor(
    private sepDataService: SpecialEventsDataService,
    public dialog: MatDialog,
    private db: IndexedDBService,
    private router: Router
  ) {}

  async ngOnInit(): Promise<void> {
    const draftApplications = await this.getDraftApplications();

    this.dataSource.data = draftApplications;

    if (this.dataSource) {
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    }
  }

  async getDraftApplications() {
    const [persistedDrafts, localDrafts] = await Promise.all([
      this.getPersistedDraftApplications(),
      this.getLocalDraftApplications(),
    ]);

    return [
      ...persistedDrafts,
      ...localDrafts.sort((a, b) => {
        const dateA = new Date(a.dateCreated).getTime();
        const dateB = new Date(b.dateCreated).getTime();
        return dateB - dateA;
      }),
    ];
  }

  async getPersistedDraftApplications(): Promise<SepDraftApplicationSummary[]> {
    return this.sepDataService.getDraftApplications().toPromise();
  }

  async getLocalDraftApplications(): Promise<SepApplication[]> {
    let allCachedApplications = await this.db.applications.toArray();

    return allCachedApplications.filter((app) => app.eventStatus === "Draft");
  }

  /**
   * Open the draft application to continue editing it.
   *
   * @param {SepDraftApplicationSummary} app
   * @return {*}
   * @memberof DraftApplicationsComponent
   */
  openApplication(app: SepDraftApplicationSummary) {
    console.log("openApplication draft", { app });
    if (app.specialEventId) {
      this.router.navigateByUrl(
        `sep/application/${app.specialEventId}/${this.getLastStep(
          app.lastStepCompleted
        )}`
      );
      return;
    }

    if (app.localId) {
      this.router.navigateByUrl(
        `sep/application/${app.localId}/${this.getLastStep(
          app.lastStepCompleted
        )}?localId=${app.localId}`
      );
    }
  }

  getLastStep(stepCompleted: string): string {
    const lastIndex = SEP_APPLICATION_STEPS.indexOf(stepCompleted);
    // return the next step to be completed
    return SEP_APPLICATION_STEPS[lastIndex + 1];
  }

  isEventPast(eventStartDate: string) {
    return isBefore(new Date(eventStartDate), new Date());
  }

  async cloneApplication(appSummary: SepDraftApplicationSummary) {
    if (this.isPersistedDraft(appSummary)) {
      this.sepDataService
        .getSpecialEventForApplicant(appSummary.specialEventId)
        .subscribe(async (app) => {
          const clone = { ...app };
          // clear dynamics IDs
          clone.id = undefined;
          clone.localId = undefined;
          clone.eventStatus = "Draft";

          // ensure the police field are cleared
          clone.policeDecisionBy = undefined;
          clone.policeApproval = undefined;

          if (clone?.eventLocations?.length > 0) {
            clone.eventLocations.forEach((loc) => {
              loc.id = undefined;
              if (loc?.serviceAreas?.length > 0) {
                loc.serviceAreas.forEach((area) => {
                  area.id = undefined;
                });
              }
              if (loc?.eventDates?.length > 0) {
                loc.eventDates.forEach((ed) => {
                  ed.id = undefined;
                });
              }
            });
          }

          const localId = await this.db.saveSepApplication({
            ...clone,
            dateAgreedToTsAndCs: undefined,
            isAgreeTsAndCs: false,
            dateCreated: new Date(),
          } as SepApplication);
          this.router.navigateByUrl(`/sep/application/${localId}/applicant`);
        });

      return;
    }

    if (this.isLocalDraft(appSummary)) {
    }
  }

  async cancelApplication(
    appSummary: SepDraftApplicationSummary
  ): Promise<void> {
    // open dialog, get reference and process returned data from dialog
    const dialogConfig = {
      disableClose: true,
      autoFocus: true,
      width: "600px",
      height: "500px",
      data: {
        showStartApp: false,
      },
    };

    const dialogRef = this.dialog.open(
      CancelSepApplicationDialogComponent,
      dialogConfig
    );

    dialogRef.afterClosed().subscribe(async ([cancelApplication, reason]) => {
      if (cancelApplication !== true) {
        return;
      }

      if (this.isPersistedDraft(appSummary)) {
        // If this draft was complete enough to be persisted, update the application with a cancelled status/reason.
        await this.sepDataService
          .updateSepApplication(
            {
              id: appSummary.specialEventId,
              cancelReason: reason,
              eventStatus: "Cancelled",
            } as SepApplication,
            appSummary.specialEventId
          )
          .toPromise();
      }

      if (appSummary.localId) {
        // If this application was cached (persisted draft or local draft), remove it from the local storage
        await this.db.applications.delete(Number(appSummary.localId));
      }

      this.router.navigateByUrl(`/sep/my-applications`).then(() => {
        window.location.reload();
      });
    });
  }

  /**
   * Check if the draft application is local only (exists only in local storage).
   *
   * @param {SepDraftApplicationSummary} appSummary
   * @return {*}  {boolean}
   * @memberof DraftApplicationsComponent
   */
  isLocalDraft(appSummary: SepDraftApplicationSummary): boolean {
    return Boolean(!appSummary.specialEventId && appSummary.localId);
  }

  /**
   * Check if the draft application is persisted (exists in dynamics, and has a primary id).
   *
   * @param {SepDraftApplicationSummary} appSummary
   * @return {*}  {boolean}
   * @memberof DraftApplicationsComponent
   */
  isPersistedDraft(appSummary: SepDraftApplicationSummary): boolean {
    return Boolean(appSummary.specialEventId);
  }
}
