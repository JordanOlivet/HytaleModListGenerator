<script lang="ts">
	import { status, refreshMods } from '$lib/stores/mods';

	async function handleRefresh() {
		await refreshMods(false);
	}

	$: isRefreshing = $status?.isRefreshing ?? false;
	$: progress = $status?.progress;
</script>

<button
	class="refresh-button"
	onclick={handleRefresh}
	disabled={isRefreshing}
	aria-label="Refresh mods"
>
	{#if isRefreshing}
		<svg class="spinner" xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
			<path d="M21 12a9 9 0 1 1-6.219-8.56"/>
		</svg>
		<span>
			{#if progress}
				{progress.processed}/{progress.total}
			{:else}
				Refreshing...
			{/if}
		</span>
	{:else}
		<svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
			<polyline points="23 4 23 10 17 10"></polyline>
			<polyline points="1 20 1 14 7 14"></polyline>
			<path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15"/>
		</svg>
		<span>Refresh</span>
	{/if}
</button>

<style>
	.refresh-button {
		display: flex;
		align-items: center;
		gap: 8px;
		padding: 8px 16px;
		background: var(--accent-color);
		color: white;
		border: none;
		border-radius: 8px;
		font-size: 14px;
		font-weight: 500;
		cursor: pointer;
		transition: all 0.2s ease;
	}

	.refresh-button:hover:not(:disabled) {
		opacity: 0.9;
	}

	.refresh-button:disabled {
		opacity: 0.7;
		cursor: not-allowed;
	}

	.spinner {
		animation: spin 1s linear infinite;
	}

	@keyframes spin {
		from { transform: rotate(0deg); }
		to { transform: rotate(360deg); }
	}
</style>
