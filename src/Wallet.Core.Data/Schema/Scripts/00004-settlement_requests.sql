CREATE TABLE IF NOT EXISTS public.settlement_requests (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL,
    amount numeric(18, 2) NOT NULL,
    bank_account_id uuid NOT NULL,
    request_status smallint NOT NULL DEFAULT 1,
    invoice_url varchar(500),
    payment_slip_url varchar(500),
    locked_amount numeric(18, 2) NOT NULL DEFAULT 0,
    completed_at timestamptz,
    version integer NOT NULL DEFAULT 1,
    status smallint NOT NULL DEFAULT 1,
    created_at timestamptz NOT NULL DEFAULT now(),
    created_by varchar(100) NOT NULL,
    modified_at timestamptz NOT NULL DEFAULT now(),
    modified_by varchar(100) NOT NULL
);

ALTER TABLE public.settlement_requests DROP CONSTRAINT IF EXISTS CHK_settlement_requests_amount;
ALTER TABLE public.settlement_requests ADD CONSTRAINT CHK_settlement_requests_amount CHECK (amount > 0);
ALTER TABLE public.settlement_requests DROP CONSTRAINT IF EXISTS CHK_settlement_requests_request_status;
ALTER TABLE public.settlement_requests ADD CONSTRAINT CHK_settlement_requests_request_status
    CHECK (request_status IN (1, 2, 3, 4));
ALTER TABLE public.settlement_requests DROP CONSTRAINT IF EXISTS CHK_settlement_requests_status;
ALTER TABLE public.settlement_requests ADD CONSTRAINT CHK_settlement_requests_status CHECK (status IN (1, 2));

CREATE INDEX IF NOT EXISTS IX_settlement_requests_user_id_status
    ON public.settlement_requests (user_id, status);
CREATE INDEX IF NOT EXISTS IX_settlement_requests_request_status
    ON public.settlement_requests (request_status);
