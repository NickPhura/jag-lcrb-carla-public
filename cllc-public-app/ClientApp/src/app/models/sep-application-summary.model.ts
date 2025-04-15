import { SepApplication } from "@models/sep-application.model";
import { Account } from "./account.model";
import { Contact } from "./contact.model";
import { SepCity } from "./sep-city.model";

/**
 * A summary (subset) of fields for a submitted special event application.
 *
 * @export
 * @class SepApplicationSummary
 */
export class SepApplicationSummary {
  localId: string; // local memory primary key
  lastStepCompleted: string;
  specialEventId: string; // server side primary key
  eventStartDate: Date | string;
  eventName: string;
  typeOfEvent: number;
  eventStatus: string;
  maximumNumberOfGuests: number;
  dateSubmitted: Date | string;
  invoiceId: string;
  isInvoicePaid: boolean;

  policeAccount?: Account;
  policeDecisionBy?: Contact;
  policeApproval?: string;
  lcrbApproval?: string;
  denialReason?: string;
  cancelReason?: string;
  dateOfPoliceDecision?: Date | string;
}

/**
 * A summary (subset) of fields for a draft special event application.
 *
 * @export
 * @class SepDraftApplicationSummary
 */
export class SepDraftApplicationSummary extends SepApplicationSummary {}

export type SepDraftApplicationTableElement = (
  | SepDraftApplicationSummary
  | SepApplication
) & {
  eventStatusLabel?: string;
  typeOfEventLabel?: string;
};
