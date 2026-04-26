CREATE TABLE IF NOT EXISTS public.trans_details (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    transaction_id uuid NOT NULL REFERENCES public.transactions (id) ON DELETE CASCADE,
    key varchar(100) NOT NULL,
    value text NOT NULL,
    version integer NOT NULL DEFAULT 1,
    status smallint NOT NULL DEFAULT 1,
    created_at timestamptz NOT NULL DEFAULT now(),
    created_by varchar(100) NOT NULL,
    modified_at timestamptz NOT NULL DEFAULT now(),
    modified_by varchar(100) NOT NULL
);

ALTER TABLE public.trans_details DROP CONSTRAINT IF EXISTS CHK_trans_details_status;
ALTER TABLE public.trans_details ADD CONSTRAINT CHK_trans_details_status CHECK (status IN (1, 2));

CREATE UNIQUE INDEX IF NOT EXISTS uq_trans_details_transaction_id_key
    ON public.trans_details (transaction_id, key) WHERE (status = 1);

CREATE INDEX IF NOT EXISTS IX_trans_details_transaction_id ON public.trans_details (transaction_id);
