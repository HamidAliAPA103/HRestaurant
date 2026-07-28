import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  Mail,
  MoreHorizontal,
  Plus,
  Search,
  UserRoundCheck,
  UsersRound,
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
import { getErrorMessage, initials } from "@/shared/lib/utils";
import type { User, UserInput } from "@/shared/types/domain";

const schema = z.object({
  name: z.string().min(2, "Ad ən azı 2 simvol olmalıdır."),
  email: z.string().email("Düzgün email daxil edin."),
  role: z.string().min(1, "Rol seçin."),
});

const roleTone: Record<
  string,
  "success" | "warning" | "danger" | "info" | "neutral"
> = {
  Manager: "danger",
  Chef: "warning",
  Waiter: "info",
  Cashier: "success",
  Host: "neutral",
};

export function EmployeePage() {
  const [modalOpen, setModalOpen] = useState(false);
  const [search, setSearch] = useState("");
  const queryClient = useQueryClient();
  const query = useQuery({
    queryKey: ["users", "employees"],
    queryFn: () => listResource<User>("/User"),
  });
  const employees = useMemo(
    () =>
      (query.data?.data ?? []).filter(
        (user) =>
          user.role.toLowerCase() !== "customer" &&
          `${user.name} ${user.email} ${user.role}`
            .toLowerCase()
            .includes(search.toLowerCase()),
      ),
    [query.data?.data, search],
  );
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<UserInput>({
    resolver: zodResolver(schema),
    defaultValues: { name: "", email: "", role: "Waiter" },
  });
  const mutation = useMutation({
    mutationFn: async (input: UserInput) => {
      const response = await createResource<UserInput>("/User", input);
      if (!response.success) throw new Error(response.message);
      return response;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
      reset();
      setModalOpen(false);
    },
  });

  return (
    <div className="page-enter space-y-6">
      <PageHeader
        eyebrow="Komanda"
        title="Əməkdaşlar"
        description="Növbədə olan komandanı izləyin, rolları və əməkdaş məlumatlarını idarə edin."
        actions={
          <Button onClick={() => setModalOpen(true)}>
            <Plus className="h-4 w-4" />
            Əməkdaş əlavə et
          </Button>
        }
      />

      <div className="grid gap-4 sm:grid-cols-3">
        {[
          {
            label: "Ümumi əməkdaş",
            value: employees.length,
            icon: UsersRound,
          },
          {
            label: "Bu gün növbədə",
            value: employees.length,
            icon: UserRoundCheck,
          },
          {
            label: "Aktiv rollar",
            value: new Set(employees.map((employee) => employee.role)).size,
            icon: UserRoundCheck,
          },
        ].map((stat) => (
          <div key={stat.label} className="card flex items-center gap-4 p-5">
            <div className="grid h-11 w-11 place-items-center rounded-2xl bg-[#f2ede6] text-[#e85d3f]">
              <stat.icon className="h-5 w-5" />
            </div>
            <div>
              <div className="text-2xl font-bold text-[#29231f]">
                {stat.value}
              </div>
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
            placeholder="Ad, email və ya rol üzrə axtar..."
            className="h-11 w-full rounded-xl border border-[#ded8d0] bg-[#faf8f5] pl-10 pr-4 text-sm outline-none focus:border-[#e85d3f]"
          />
        </label>
      </div>

      {query.isLoading ? (
        <LoadingState label="Əməkdaşlar yüklənir" />
      ) : query.isError ? (
        <ErrorState
          message={getErrorMessage(query.error)}
          onRetry={() => query.refetch()}
        />
      ) : employees.length === 0 ? (
        <EmptyState
          title={search ? "Uyğun əməkdaş tapılmadı" : "Əməkdaş yoxdur"}
          description={
            search
              ? "Axtarış sözünü dəyişərək yenidən yoxlayın."
              : "Komandanıza ilk əməkdaşı əlavə edin."
          }
        />
      ) : (
        <div className="table-shell overflow-x-auto">
          <table className="data-table min-w-[720px]">
            <thead>
              <tr>
                <th>Əməkdaş</th>
                <th>Rol</th>
                <th>Status</th>
                <th>Əlaqə</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {employees.map((employee) => (
                <tr key={employee.id}>
                  <td>
                    <div className="flex items-center gap-3">
                      <div className="grid h-10 w-10 place-items-center rounded-xl bg-[#efeae3] text-xs font-bold text-[#4e4640]">
                        {initials(employee.name)}
                      </div>
                      <div>
                        <div className="font-bold text-[#302a26]">
                          {employee.name}
                        </div>
                        <div className="mt-0.5 text-xs text-[#91877f]">
                          ID {employee.id.slice(0, 6)}
                        </div>
                      </div>
                    </div>
                  </td>
                  <td>
                    <Badge tone={roleTone[employee.role] ?? "neutral"}>
                      {employee.role}
                    </Badge>
                  </td>
                  <td>
                    <Badge tone="success" dot>
                      Növbədə
                    </Badge>
                  </td>
                  <td>
                    <span className="inline-flex items-center gap-2 text-[#655d56]">
                      <Mail className="h-3.5 w-3.5 text-[#9d938b]" />
                      {employee.email}
                    </span>
                  </td>
                  <td>
                    <button className="grid h-8 w-8 place-items-center rounded-lg hover:bg-[#f0ece6]">
                      <MoreHorizontal className="h-4 w-4" />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <Modal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        title="Yeni əməkdaş"
        description="Əməkdaş profilini və sistem rolunu təyin edin."
      >
        <form
          className="space-y-4"
          onSubmit={handleSubmit((values) => mutation.mutate(values))}
        >
          <FormField
            label="Ad və soyad"
            placeholder="Əməkdaşın tam adı"
            error={errors.name?.message}
            {...register("name")}
          />
          <FormField
            label="Email"
            type="email"
            placeholder="employee@restaurant.az"
            error={errors.email?.message}
            {...register("email")}
          />
          <label className="block">
            <span className="mb-2 block text-sm font-semibold text-[#3c3530]">
              Rol
            </span>
            <select
              className="h-12 w-full rounded-xl border border-[#dcd5cc] bg-white px-4 text-sm outline-none focus:border-[#e85d3f]"
              {...register("role")}
            >
              <option>Manager</option>
              <option>Chef</option>
              <option>Waiter</option>
              <option>Cashier</option>
              <option>Host</option>
            </select>
          </label>
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
              Əlavə et
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
