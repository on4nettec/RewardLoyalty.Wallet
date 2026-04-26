CREATE TABLE IF NOT EXISTS public.wallets (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL,
    main_balance numeric(18, 2) NOT NULL DEFAULT 0,
    cashback_balance numeric(18, 2) NOT NULL DEFAULT 0,
    total_main_deposited numeric(18, 2) NOT NULL DEFAULT 0,
    total_main_withdrawn numeric(18, 2) NOT NULL DEFAULT 0,
    total_cashback_received numeric(18, 2) NOT NULL DEFAULT 0,
    total_cashback_spent numeric(18, 2) NOT NULL DEFAULT 0,
    is_locked boolean NOT NULL DEFAULT false,
    locked_until timestamptz,
    version integer NOT NULL DEFAULT 1,
    status smallint NOT NULL DEFAULT 1,
    created_at timestamptz NOT NULL DEFAULT now(),
    created_by varchar(100) NOT NULL,
    modified_at timestamptz NOT NULL DEFAULT now(),
    modified_by varchar(100) NOT NULL
);

ALTER TABLE public.wallets DROP CONSTRAINT IF EXISTS CHK_wallets_main_balance;
ALTER TABLE public.wallets ADD CONSTRAINT CHK_wallets_main_balance CHECK (main_balance >= 0);
ALTER TABLE public.wallets DROP CONSTRAINT IF EXISTS CHK_wallets_status;
ALTER TABLE public.wallets ADD CONSTRAINT CHK_wallets_status CHECK (status IN (1, 2));

CREATE UNIQUE INDEX IF NOT EXISTS uq_wallets_user_id_active
    ON public.wallets (user_id) WHERE (status = 1);

CREATE INDEX IF NOT EXISTS IX_wallets_user_id_status ON public.wallets (user_id, status);
CREATE INDEX IF NOT EXISTS IX_wallets_status ON public.wallets (status);
