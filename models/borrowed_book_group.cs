using System;
using System.Collections.Generic;
using System.Globalization;

namespace oop_project.Models
{
    /// <summary>
    /// DISPLAY GROUPING: every copy of a loan is still its own row in
    /// borrow_transactions_table, but the BORROWED BOOKS tab shows the copies of
    /// one title taken by one borrower as a single line. BORROWED COPY then holds
    /// the number of copies in the group instead of a wall of 1s.
    /// </summary>
    public sealed class BorrowedBookGroup
    {
        private readonly List<BorrowRecord> _records;

        public BorrowedBookGroup(IEnumerable<BorrowRecord> records)
        {
            if (records == null)
            {
                throw new ArgumentNullException(nameof(records));
            }

            _records = new List<BorrowRecord>(records);

            if (_records.Count == 0)
            {
                throw new ArgumentException("A borrow group needs at least one record.", nameof(records));
            }

            // Oldest copy first, so the BORROWED column reports when the loan began
            // and the record ids are listed in the order MySQL created them.
            _records.Sort(delegate (BorrowRecord left, BorrowRecord right)
            {
                int byDate = left.DateBorrowed.CompareTo(right.DateBorrowed);

                return byDate != 0 ? byDate : left.BorrowId.CompareTo(right.BorrowId);
            });
        }

        /// <summary>
        /// Builds one group per borrower + title + borrow date, keeping the catalog
        /// order.
        /// </summary>
        public static List<BorrowedBookGroup> BuildGroups(IEnumerable<BorrowRecord> records)
        {
            List<BorrowedBookGroup> groups = new List<BorrowedBookGroup>();

            if (records == null)
            {
                return groups;
            }

            Dictionary<string, List<BorrowRecord>> buckets =
                new Dictionary<string, List<BorrowRecord>>(StringComparer.Ordinal);
            List<string> order = new List<string>();

            foreach (BorrowRecord record in records)
            {
                if (record == null)
                {
                    continue;
                }

                string key = KeyFor(record);
                List<BorrowRecord>? bucket;

                if (!buckets.TryGetValue(key, out bucket))
                {
                    bucket = new List<BorrowRecord>();
                    buckets.Add(key, bucket);
                    order.Add(key);
                }

                bucket.Add(record);
            }

            foreach (string key in order)
            {
                groups.Add(new BorrowedBookGroup(buckets[key]));
            }

            return groups;
        }

        private static string KeyFor(BorrowRecord record)
        {
            // A borrower saved in the session only (MySQL offline) has no id yet, so
            // the ID NUMBER and NAME stand in for it and two different people never
            // end up merged into one line.
            string borrower = record.BorrowerId > 0
                ? "ID:" + record.BorrowerId.ToString(CultureInfo.InvariantCulture)
                : "NAME:" + record.BorrowerIdNumber + "|" + record.BorrowerName;

            string book = record.BookId > 0
                ? "BOOK:" + record.BookId.ToString(CultureInfo.InvariantCulture)
                : "TITLE:" + record.BookTitle.ToUpperInvariant();

            // One borrow event per calendar day: the copies approved together share a
            // date, while a title taken again on a later day stays a separate line.
            string loanDate = record.DateBorrowed.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

            return borrower + "|" + book + "|" + loanDate;
        }

        public IReadOnlyList<BorrowRecord> Records
        {
            get { return _records; }
        }

        public BorrowRecord FirstRecord
        {
            get { return _records[0]; }
        }

        public int CopyCount
        {
            get { return _records.Count; }
        }

        public string CopyCountText
        {
            get { return CopyCount.ToString(CultureInfo.InvariantCulture); }
        }

        public int BookId
        {
            get { return FirstRecord.BookId; }
        }

        public string BookTitle
        {
            get { return FirstRecord.BookTitle; }
        }

        public string Author
        {
            get { return FirstRecord.Author; }
        }

        public string BookYearText
        {
            get { return FirstRecord.BookYearText; }
        }

        public string BorrowerName
        {
            get { return FirstRecord.BorrowerName; }
        }

        public string BorrowerIdNumber
        {
            get { return FirstRecord.BorrowerIdNumber; }
        }

        public string ProgramText
        {
            get { return FirstRecord.ProgramText; }
        }

        public string InstituteText
        {
            get { return FirstRecord.InstituteText; }
        }

        /// <summary>The first record id, so the table keeps one short BORROW ID.</summary>
        public string BorrowIdText
        {
            get
            {
                return FirstRecord.BorrowId > 0
                    ? FirstRecord.BorrowId.ToString(CultureInfo.InvariantCulture)
                    : "—";
            }
        }

        /// <summary>All record ids behind the line, shown as a tooltip.</summary>
        public string BorrowIdListText
        {
            get
            {
                List<string> ids = new List<string>();

                foreach (BorrowRecord record in _records)
                {
                    if (record.BorrowId > 0)
                    {
                        ids.Add("#" + record.BorrowId.ToString(CultureInfo.InvariantCulture));
                    }
                }

                return ids.Count == 0 ? "NO STORED RECORD ID" : string.Join(", ", ids);
            }
        }

        public int OpenCount
        {
            get
            {
                int count = 0;

                foreach (BorrowRecord record in _records)
                {
                    if (!record.DateReturned.HasValue)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public int ReturnedCount
        {
            get { return CopyCount - OpenCount; }
        }

        public bool IsFullyReturned
        {
            get { return OpenCount == 0; }
        }

        public List<BorrowRecord> OpenRecords
        {
            get
            {
                List<BorrowRecord> open = new List<BorrowRecord>();

                foreach (BorrowRecord record in _records)
                {
                    if (!record.DateReturned.HasValue)
                    {
                        open.Add(record);
                    }
                }

                return open;
            }
        }

        public DateTime DateBorrowed
        {
            get { return FirstRecord.DateBorrowed; }
        }

        public string DateBorrowedText
        {
            get { return FirstRecord.DateBorrowedText; }
        }

        public string DateBorrowedStampText
        {
            get { return FirstRecord.DateBorrowedStampText; }
        }

        /// <summary>The longest running copy in the group.</summary>
        public int DaysBorrowed
        {
            get
            {
                int most = 0;

                foreach (BorrowRecord record in _records)
                {
                    if (record.DaysBorrowed > most)
                    {
                        most = record.DaysBorrowed;
                    }
                }

                return most;
            }
        }

        public string DaysBorrowedText
        {
            get
            {
                return DaysBorrowed.ToString(CultureInfo.InvariantCulture) +
                       (DaysBorrowed == 1 ? " DAY" : " DAYS");
            }
        }

        /// <summary>The last return date, only once every copy is back on the shelf.</summary>
        public DateTime? DateReturned
        {
            get
            {
                if (!IsFullyReturned)
                {
                    return null;
                }

                DateTime latest = _records[0].DateReturned ?? DateTime.MinValue;

                foreach (BorrowRecord record in _records)
                {
                    DateTime returned = record.DateReturned ?? DateTime.MinValue;

                    if (returned > latest)
                    {
                        latest = returned;
                    }
                }

                return latest;
            }
        }

        public string DateReturnedText
        {
            get
            {
                DateTime? returned = DateReturned;

                return returned.HasValue
                    ? returned.Value.ToString("MMM dd, yyyy", CultureInfo.InvariantCulture).ToUpperInvariant()
                    : "—";
            }
        }

        public string Status
        {
            get { return IsFullyReturned ? "RETURNED" : "BORROWED"; }
        }
    }
}
