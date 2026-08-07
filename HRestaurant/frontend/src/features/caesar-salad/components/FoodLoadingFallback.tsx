export function FoodLoadingFallback({ message = "3D təcrübə hazırlanır…" }: { message?: string }) {
  return <div className="grid h-full min-h-72 place-items-center bg-[#160c10] px-6 text-center text-[#e3c281]"><div><div className="mx-auto h-9 w-9 animate-spin rounded-full border-2 border-[#e3c281]/25 border-t-[#e3c281]" /><p className="mt-4 text-sm">{message}</p></div></div>;
}
