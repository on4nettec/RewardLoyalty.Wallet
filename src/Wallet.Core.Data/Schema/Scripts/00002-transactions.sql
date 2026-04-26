CREATE TABLE IF NOT EXISTS public.transactions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL,
    wallet_type smallint NOT NULL,
    transaction_type smallint NOT NULL,
    reference_id uuid,
    reference_type varchar(50),
    debit numeric(18, 2) NOT NULL DEFAULT 0,
    credit numeric(18, 2) NOT NULL DEFAULT 0,
    balance_before numeric(18, 2) NOT NULL,
    balance_after numeric(18, 2) NOT NULL,
    description varchar(500) NOT NULL DEFAULT '',
    transaction_status smallint NOT NULL DEFAULT 1,
    expires_at timestamptz,
    is_expired boolean NOT NULL DEFAULT false,
    completed_at timestamptz,
    version integer NOT NULL DEFAULT 1,
    status smallint NOT NULL DEFAULT 1,
    created_at timestamptz NOT NULL DEFAULT now(),
    created_by varchar(100) NOT NULL,
    modified_at timestamptz NOT NULL DEFAULT now(),
    modified_by varchar(100) NOT NULL
);

ALTER TABLE public.transactions DROP CONSTRAINT IF EXISTS CHK_transactions_debit_credit;
ALTER TABLE public.transactions ADD CONSTRAINT CHK_transactions_debit_credit
    CHECK ((debit > 0 AND credit = 0) OR (credit > 0 AND debit = 0));
ALTER TABLE public.transactions DROP CONSTRAINT IF EXISTS CHK_transactions_balance_after_main;
ALTER TABLE public.transactions ADD CONSTRAINT CHK_transactions_balance_after_main
    CHECK (wallet_type <> 1 OR balance_after >= 0);
ALTER TABLE public.transactions DROP CONSTRAINT IF EXISTS CHK_transactions_wallet_type;
ALTER TABLE public.transactions ADD CONSTRAINT CHK_transactions_wallet_type CHECK (wallet_type IN (1, 2));
ALTER TABLE public.transactions DROP CONSTRAINT IF EXISTS CHK_transactions_transaction_status;
ALTER TABLE public.transactions ADD CONSTRAINT CHK_transactions_transaction_status
    CHECK (transaction_status IN (1, 2, 3, 4));
ALTER TABLE public.transactions DROP CONSTRAINT IF EXISTS CHK_transactions_status;
ALTER TABLE public.transactions ADD CONSTRAINT CHK_transactions_status CHECK (status IN (1, 2));

CREATE INDEX IF NOT EXISTS IX_transactions_user_id_created_at
    ON public.transactions (user_id, created_at DESC);
CREATE INDEX IF NOT EXISTS IX_transactions_user_id_transaction_type
    ON public.transactions (user_id, transaction_type);
CREATE INDEX IF NOT EXISTS IX_transactions_user_id_transaction_status
    ON public.transactions (user_id, transaction_status);
CREATE INDEX IF NOT EXISTS IX_transactions_reference
    ON public.transactions (reference_type, reference_id) WHERE (reference_id IS NOT NULL);
CREATE INDEX IF NOT EXISTS IX_transactions_status ON public.transactions (status);
