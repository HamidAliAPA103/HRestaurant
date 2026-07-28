import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  CalendarDays,
  Clock3,
  Plus,
  Search,
  Users,
} from "lucide-react";
import { useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import {
  createResource,
  listResource,
} from "@/shared/api/resources";
import { Badge } from "@/shared/components/Badge";
import { Button } from "@/shared/components/Button";
import { FormField } from "@/shared/components/FormField";
import { Modal } from "@/shared/components/Modal";
import { PageHeader } from "@/shared/components/PageHeader";
import {
  EmptyState,
  ErrorState,
  LoadingState,
} from "@/shared/components/StatePanel";
import {
  formatDate,
  getErrorMessage,
  shortId,
} from "@/shared/lib/utils";
import {
  ReservationStatus,
  type DiningTable,
  type Reservation,
  type ReservationInput,
  type User,
} from "@/shared/types/domain";

const schema = z.object({
  customerId: z.string().min(1, "Müştəri seçin."),
  tableId: z.string().min(1, "Masa seçin."),
  reservationTime: z.string().min(1, "Tarix və saat seçin."),
  guestCount: z
    .number()
    .int()
    .min(1, "Qonaq sayı ən azı 1 olmalıdır."),
});

type ReservationForm = z.infer<typeof schema>;

const statusMeta = {
  [ReservationStatus.Pending]: { label: "Gözləyir", tone: "warning" },
  [ReservationStatus.Confirmed]: { label: "Təsdiqlənib", tone: "success" },
  [ReservationStatus.Cancelled]: { label: "Ləğv edilib", tone: "danger" },
  [ReservationStatus.Completed]: { label: "Tamamlanıb", tone: "neutral" },
} as const;

export function ReservationPage() {
  const [modalOpen, setModalOpen] = useState(false);
  const [search, setSearch] = useState("");
  const queryClient = useQueryClient();
  const reservationsQuery = useQuery({
    queryKey: ["reservations"],
    queryFn: () => listResource<Reservation>("/Reservation"),
  });
  const usersQuery = useQuery({
    queryKey: ["users", "reservation"],
    queryFn: () => listResource<User>("/User"),
  });
  const tablesQuery = useQuery({
    queryKey: ["tables", "reservation"],
    queryFn: () => listResource<DiningTable>("/Table"),
  });
  const customers = (usersQuery.data?.data ?? []).filter(
    (user) => user.role.toLowerCase() === "customer",
  );
  const customerMap = Object.fromEntries(
    customers.map((user) => [user.id, user.name]),
  );
  const reservations = useMemo(
    () =>
      (reservationsQuery.data?.data ?? []).filter((reservation) =>
        `${customerMap[reservation.customerId] ?? reservation.customerId}`
          .toLowerCase()
          .includes(search.toLowerCase()),
      ),
    [customerMap, reservationsQuery.data?.data, search],
  );
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ReservationForm>({
    resolver: zodResolver(schema),
    defaultValues: {
      customerId: "",
      tableId: "",
      reservationTime: "",
      guestCount: 2,
    },
  });
  const mutation = useMutation({
    mutationFn: async (values: ReservationForm) => {
      const input: ReservationInput = {
        ...values,
        reservationTime: new Date(values.reservationTime).toISOString(),
        status: ReservationStatus.Pending,
      };
      const response = await createResource<ReservationInput>(
        "/Reservation",
        input,
      );
      if (!response.success) throw new Error(response.message);
      return response;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["reservations"] });
      reset();
      setModalOpen(false);
    },
  });

  return (
    <div className="page-enter space-y-6">
      <PageHeader
        eyebrow="Qonaq planı"
        title="Rezervasiyalar"
        description="Gələcək qonaqları, masa təyinatlarını və rezervasiya statuslarını idarə edin."
        actions={
          <Button onClick={() => setModalOpen(true)}>
            <Plus className="h-4 w-4" />
            Yeni rezervasiya
          </Button>
        }
      />

      <div className="grid gap-4 sm:grid-cols-3">
        {[
          {
            label: "Bu gün",
            value: reservations.length,
            icon: CalendarDays,
          },
          {
            label: "Təsdiqlənən",
            value: reservations.filter(
              (item) => item.status === ReservationStatus.Confirmed,
            ).length,
            icon: Clock3,
          },
          {
            label: "Qonaq sayı",
            value: reservations.reduce(
              (sum, item) => sum + item.guestCount,
              0,
            ),
            icon: Users,
          },
        ].map((stat) => (
          <div key={stat.label} className="card flex items-center gap-4 p-5">
            <div className="grid h-11 w-11 place-items-center rounded-2xl bg-[#f2ede6] text-[#e85d3f]">
              <stat.icon className="h-5 w-5" />
            </div>
            <div>
              <div className="text-2xl font-bold">{stat.value}</div>
              <div className="text-xs text-[#877d75]">{stat.label}</div>
            </div>
          </div>
        ))}
      </div>

      <div className="card p-4">
        <label className="relative block max-w-md">
          <Search className="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-[#968d85]" />
          <input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Müştəri adı ilə axtar..."
            className="h-11 w-full rounded-xl border border-[#ded8d0] bg-[#faf8f5] pl-10 pr-4 text-sm outline-none focus:border-[#e85d3f]"
          />
        </label>
      </div>

      {reservationsQuery.isLoading ? (
        <LoadingState label="Rezervasiyalar yüklənir" />
      ) : reservationsQuery.isError ? (
        <ErrorState
          message={getErrorMessage(reservationsQuery.error)}
          onRetry={() => reservationsQuery.refetch()}
        />
      ) : reservations.length === 0 ? (
        <EmptyState title="Rezervasiya tapılmadı" />
      ) : (
        <div className="table-shell overflow-x-auto">
          <table className="data-table min-w-[760px]">
            <thead>
              <tr>
                <th>Qonaq</th>
                <th>Tarix və saat</th>
                <th>Masa</th>
                <th>Qonaq sayı</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {reservations.map((reservation) => {
                const meta = statusMeta[reservation.status];
                const tableIndex =
                  (tablesQuery.data?.data ?? []).findIndex(
                    (table) => table.id === reservation.tableId,
                  ) + 1;
                return (
                  <tr key={reservation.id}>
                    <td>
                      <div className="font-bold text-[#302a26]">
                        {customerMap[reservation.customerId] ??
                          shortId(reservation.customerId)}
                      </div>
                      <div className="mt-0.5 text-xs text-[#91877f]">
                        {shortId(reservation.id)}
                      </div>
                    </td>
                    <td>{formatDate(reservation.reservationTime, true)}</td>
                    <td>Masa {tableIndex || "—"}</td>
                    <td>
                      <span className="inline-flex items-center gap-1.5">
                        <Users className="h-3.5 w-3.5 text-[#9a9088]" />
                        {reservation.guestCount} nəfər
                      </span>
                    </td>
                    <td>
                      <Badge tone={meta.tone} dot>
                        {meta.label}
                      </Badge>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}

      <Modal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        title="Yeni rezervasiya"
        description="Qonaq, vaxt və masa məlumatlarını seçin."
      >
        <form
          className="space-y-4"
          onSubmit={handleSubmit((values) => mutation.mutate(values))}
        >
          <label className="block">
            <span className="mb-2 block text-sm font-semibold">Müştəri</span>
            <select
              className="h-12 w-full rounded-xl border border-[#dcd5cc] bg-white px-4 text-sm outline-none focus:border-[#e85d3f]"
              {...register("customerId")}
            >
              <option value="">Müştəri seçin</option>
              {customers.map((customer) => (
                <option key={customer.id} value={customer.id}>
                  {customer.name}
                </option>
              ))}
            </select>
            {errors.customerId && (
              <span className="mt-1.5 block text-xs text-[#c94a33]">
                {errors.customerId.message}
              </span>
            )}
          </label>
          <div className="grid gap-4 sm:grid-cols-2">
            <label className="block">
              <span className="mb-2 block text-sm font-semibold">Masa</span>
              <select
                className="h-12 w-full rounded-xl border border-[#dcd5cc] bg-white px-4 text-sm outline-none focus:border-[#e85d3f]"
                {...register("tableId")}
              >
                <option value="">Masa seçin</option>
                {(tablesQuery.data?.data ?? []).map((table, index) => (
                  <option key={table.id} value={table.id}>
                    Masa {index + 1} · {table.tutum} nəfər
                  </option>
                ))}
              </select>
            </label>
            <FormField
              label="Qonaq sayı"
              type="number"
              min={1}
              error={errors.guestCount?.message}
              {...register("guestCount", { valueAsNumber: true })}
            />
          </div>
          <FormField
            label="Tarix və saat"
            type="datetime-local"
            error={errors.reservationTime?.message}
            {...register("reservationTime")}
          />
          {mutation.isError && (
            <p className="rounded-xl bg-[#fff0ed] p-3 text-sm text-[#b5442f]">
              {getErrorMessage(mutation.error)}
            </p>
          )}
          <div className="flex justify-end gap-2 pt-2">
            <Button
              type="button"
              variant="secondary"
              onClick={() => setModalOpen(false)}
            >
              Ləğv et
            </Button>
            <Button type="submit" loading={mutation.isPending}>
              Rezervasiya yarat
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
