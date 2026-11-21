# 3NF Normalization — TCGC Library Management System

The `libsys_database` schema is in Third Normal Form:

- every table has a surrogate primary key and atomic columns;
- repeated role, status and school values are lookup foreign keys;
- derived values are calculated instead of stored.

## Changes

| Area | 3NF fix |
|---|---|
| Roles / account status | `role_table`, `account_status_table`; `account_role_table` stores `role_id`, `status_id`, `date_created`. |
| Request status | `request_status_table`; `borrow_requests_table.status_id`. |
| Student institute | Derived through `program_table.institute_id`; `student_info_table` stores only `program_id`. |
| Borrower affiliation | `borrowers_table` stores `program_id` (the normalised key) and the resolved `institute_id`, so every borrower row records both affiliations. |
| Book values | `copies` is `INT`, `date_added` is `DATE`; availability is derived from open loans. |
| Loan state | `BORROWED` / `RETURNED` and the number of days are derived from `date_borrow` / `date_return`. |
| Evaluation respondent | `evaluation_table` keeps only the submission (`evaluation_id`, `account_id`, `period_id`, `submitted_at`). Role and name are stored once in `evaluation_respondent_table`. |
| Evaluation period | `is_open` is a stored generated column derived from `opened_at` / `closed_at`; `ux_single_open_period` allows at most one open period. |
| Evaluation answers | `evaluation_question_table` stores every statement once; `evaluation_answer_table` references `question_id`. |

## Tables

Lookup: `role_table`, `account_status_table`, `request_status_table`, `institute_table`, `program_table`.

Main: `account_table`, `account_role_table`, `student_info_table`, `faculty_info_table`, `staff_info_table`, `librarian_info_table`, `books_table`, `borrowers_table`, `borrow_transactions_table`, `borrow_requests_table`, `attendance_table`, `evaluation_period_table`, `evaluation_question_table`, `evaluation_table`, `evaluation_respondent_table`, `evaluation_answer_table`.

## Code changes

- `models/account_directory.cs` — lookup joins and inserts; student institute read through the program; name propagation includes `evaluation_respondent_table`.
- `models/library_catalog.cs` — numeric copies, date values, borrower foreign keys; no stored display name or day counter.
- `models/borrow_request_directory.cs` — status lookup and book copies.
- `models/attendance_directory.cs` — role read from `account_role_table`.
- `models/evaluation_directory.cs` — respondent split, generated `is_open`, runtime schema upgrade.
- `forms/evaluation_login.cs`, `forms/login.cs`, `forms/signup.cs` — lookup-based role/status SQL.

## Applying the schema

Fresh database:

```
mysql -u root -P 3306 < database/libsys_database.sql
```

Existing database — back it up first:

```
mysqldump -u root -P 3306 --hex-blob --result-file=libsys_backup.sql libsys_database
mysql -u root -P 3306 < database/normalize_to_3nf.sql
```

The script builds `libsys_database_3nf` without changing the source. After verifying it:

```
mysqldump -u root -P 3306 --hex-blob --result-file=libsys_3nf.sql libsys_database_3nf
mysql -u root -P 3306 -e "DROP DATABASE libsys_database; CREATE DATABASE libsys_database DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;"
mysql -u root -P 3306 --binary-mode=1 libsys_database < libsys_3nf.sql
```

## Verification

```sql
-- Role and status resolve through lookup tables.
SELECT a.account_id, a.name, rt.role_code, st.status_code, ar.date_created
FROM account_table a
JOIN account_role_table ar ON ar.account_id = a.account_id
JOIN role_table rt ON rt.role_id = ar.role_id
JOIN account_status_table st ON st.status_id = ar.status_id;

-- Student institute comes from the program.
SELECT s.student_info_id, s.school_id, p.program_name, i.institute_code
FROM student_info_table s
JOIN program_table p ON p.program_id = s.program_id
JOIN institute_table i ON i.institute_id = p.institute_id;

-- One respondent row per evaluation.
SELECT e.evaluation_id, er.role_id, er.respondent_name, e.submitted_at
FROM evaluation_table e
JOIN evaluation_respondent_table er ON er.evaluation_id = e.evaluation_id;

-- is_open is derived, and only one period is open.
SELECT period_id, semester, is_open, opened_at, closed_at
FROM evaluation_period_table
ORDER BY period_id;
```
