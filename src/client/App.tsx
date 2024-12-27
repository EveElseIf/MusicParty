import { useState } from "react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query"
import { trpc } from "./utils/trpc";
import { httpBatchLink } from "@trpc/react-query";
import { BrowserRouter, Route, Routes } from "react-router";
import { Room } from "./components/Room";
import { Home } from "./components/Home";

function App() {
  const [queryClient] = useState(() => new QueryClient({
    defaultOptions: {
      mutations: {
        retry: 3,
        retryDelay: (att) => Math.min(1000 * 2 ** att, 30000)
      }
    }
  }));
  const [trpcClient] = useState(() =>
    trpc.createClient({
      links: [
        httpBatchLink({
          url: location.origin + '/trpc',
          // You can pass any HTTP headers you wish here
        }),
      ],
    }),
  );

  return (
    <>
      <trpc.Provider client={trpcClient} queryClient={queryClient}>
        <QueryClientProvider client={queryClient}>
          <BrowserRouter>
            <Routes>
              <Route path="/" element={<Home />} />
              <Route path="/room/:roomId" element={<Room />} />
            </Routes>
          </BrowserRouter>
        </QueryClientProvider>
      </trpc.Provider>
    </>
  );
}

export default App;
