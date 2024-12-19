import { useState } from "react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query"
import Home from "./components/Home";
import { trpc } from "./utils/trpc";
import { httpBatchLink } from "@trpc/react-query";

function App() {
  const [queryClient] = useState(() => new QueryClient());
  const [trpcClient] = useState(() =>
    trpc.createClient({
      links: [
        httpBatchLink({
          url: location.origin + '/trpc',
          // You can pass any HTTP headers you wish here
          async headers() {
            return {
              // authorization: getAuthCookie(),
            };
          },
        }),
      ],
    }),
  );

  return (
    <>
      <trpc.Provider client={trpcClient} queryClient={queryClient}>
        <QueryClientProvider client={queryClient}>
          <Home />
        </QueryClientProvider>
      </trpc.Provider>
    </>
  );
}

export default App;
